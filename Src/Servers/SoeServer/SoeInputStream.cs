namespace Servers.SOEInputStream
{
    using H1EmuCore;
    using Servers.SOEOutputStream;
    using System.Diagnostics;
    using System.Text;
    using Utils.Constants;

    public class WrappedUint16
    {
        private const int MAX_UINT16 = 65535;
        private int value;

        public WrappedUint16(int initValue)
        {
            if (initValue > MAX_UINT16)
            {
                throw new Exception("WrappedUint16 can only hold values up to 65535");
            }
            value = initValue;
        }

        public static int Wrap(int value)
        {
            int uint16 = value;
            if (uint16 > MAX_UINT16)
            {
                uint16 -= MAX_UINT16 + 1;
            }
            return uint16;
        }

        public void Add(int value)
        {
            value = Wrap(value + value);
        }

        public void Set(int value)
        {
            value = Wrap(value);
        }

        public int Get()
        {
            return value;
        }

        public void Increment()
        {
            Add(1);
        }
    }

    public class EventEmitter
    {
        private readonly Dictionary<string, List<Action<object[]>>> _eventListeners;

        public EventEmitter()
        {
            _eventListeners = new Dictionary<string, List<Action<object[]>>>();
        }

        public void On(string eventName, Action<object[]> callback)
        {
            if (!_eventListeners.ContainsKey(eventName))
            {
                _eventListeners[eventName] = new List<Action<object[]>>();
            }

            _eventListeners[eventName].Add(callback);
        }

        public void Emit(string eventName, params object[] args)
        {
            if (_eventListeners.ContainsKey(eventName))
            {
                foreach (var callback in _eventListeners[eventName])
                {
                    callback(args);
                }
            }
        }
    }
    public class Fragment
    {
        public byte[] Payload { get; set; }
        public bool IsFragment { get; set; }
    }

    public class SOEInputStream : EventEmitter
    {
        private readonly WrappedUint16 _nextSequence = new WrappedUint16(0);
        private readonly WrappedUint16 _lastAck = new WrappedUint16(-1);
        private readonly Dictionary<int, Fragment> _fragments = new Dictionary<int, Fragment>();
        private bool _useEncryption = false;
        int _LastProcessedSequence = -1;
        private readonly RC4 _rc4;
        private bool _hasCPF = false;
        private int _cpfTotalSize = -1;
        private int _cpfDataSize = -1;
        private byte[] _cpfDataWithoutHeader = null;
        private List<int> _cpfProcessedFragmentsSequences = new List<int>();

        public SOEInputStream(byte[] cryptoKey)
        {
            _useEncryption = cryptoKey != null;
            _rc4 = _useEncryption ? new RC4(cryptoKey) : null;
        }

        private List<byte[]> ProcessSingleData(Fragment dataToProcess, int sequence)
        {
            _fragments.Remove(sequence);
            _LastProcessedSequence = sequence;
            return ParseChannelPacketData(dataToProcess.Payload);
        }

        private List<byte[]> processFragmentedData(int firstPacketSequence)
        {
            // cpf == current processed fragment
            if (!_hasCPF)
            {
                Fragment firstPacket = (Fragment)_fragments[firstPacketSequence]; // should be always defined
                // the total size is written has a uint32 at the first packet of a fragmented data
                _cpfTotalSize = (int)BitConverter.ToUInt32(firstPacket.Payload, 0);
                _cpfDataSize = 0;

                _cpfDataWithoutHeader = new byte[_cpfTotalSize];
                _cpfProcessedFragmentsSequences = new List<int>();
                _hasCPF = true;
            }
            for (int i = _cpfProcessedFragmentsSequences.Count; i < _fragments.Count; i++)
            {
                int fragmentSequence = (int)((firstPacketSequence + i) % Constants.MAX_SEQUENCE);
                Fragment fragment = (Fragment)_fragments[fragmentSequence];
                if (fragment != null)
                {
                    bool isFirstPacket = fragmentSequence == firstPacketSequence;
                    _cpfProcessedFragmentsSequences.Add(fragmentSequence);
                    Array.Copy(
                        fragment.Payload,
                        isFirstPacket ? Constants.MAX_HEADER_SIZE : 0,
                        _cpfDataWithoutHeader,
                        _cpfDataSize,
                        isFirstPacket ? fragment.Payload.Length - 4 : fragment.Payload.Length
                    );
                    int fragmentDataLen = isFirstPacket
                        ? fragment.Payload.Length - 4
                        : fragment.Payload.Length;
                    _cpfDataSize += fragmentDataLen;

                    if (_cpfDataSize > _cpfTotalSize)
                    {
                        Emit(
                            "error",
                            new Exception(
                                "processDataFragments: offset > totalSize: "
                                    + _cpfDataSize
                                    + " > "
                                    + _cpfTotalSize
                                    + " (sequence "
                                    + fragmentSequence
                                    + ") (fragment length "
                                    + fragment.Payload.Length
                                    + ")"
                            )
                        );
                    }
                    if (_cpfDataSize == _cpfTotalSize)
                    {
                        // Delete all the processed fragments from memory
                        for (int k = 0; k < _cpfProcessedFragmentsSequences.Count; k++)
                        {
                            _fragments.Remove(_cpfProcessedFragmentsSequences[k]);
                        }
                        _LastProcessedSequence = fragmentSequence;
                        _hasCPF = false;
                        // process the full reassembled data
                        return ParseChannelPacketData(_cpfDataWithoutHeader);
                    }
                }
                else
                {
                    return new List<byte[]>(); // the full data hasn't been received yet
                }
            }
            return new List<byte[]>(); // if somehow there is no fragments in memory
        }

        private void _processData()
        {
            int nextFragmentSequence = (int)((_LastProcessedSequence + 1) & Constants.MAX_SEQUENCE);
            Fragment dataToProcess = (Fragment)_fragments[nextFragmentSequence];
            List<byte[]> appData = new List<byte[]>();
            if (dataToProcess != null)
            {
                if (dataToProcess.IsFragment)
                {
                    appData = processFragmentedData(nextFragmentSequence);
                }
                else
                {
                    appData = ProcessSingleData(dataToProcess, nextFragmentSequence);
                }

                if (appData.Count > 0)
                {
                    if (_fragments.ContainsKey(_LastProcessedSequence + 1))
                    {
                        Task.Run(
                            () =>
                            {
                                _processData();
                            }
                        );
                    }
                    processAppData(appData);
                }
            }
        }

        private void processAppData(List<byte[]> appData)
        {
            for (int i = 0; i < appData.Count; i++)
            {
                byte[] data = appData[i];
                if (_useEncryption)
                {
                    // sometimes there's an extra 0x00 byte in the beginning that trips up the RC4 decyption
                    if (data.Length > 1 && BitConverter.ToUInt16(data, 0) == 0)
                    {
                        data = _rc4.Encrypt(data.Skip(1).ToArray());
                    }
                    else
                    {
                        data = _rc4.Encrypt(data);
                    }
                }
                Emit("appdata", data); // sending appdata to application
            }
        }

        private bool acknowledgeInputData(int sequence)
        {
            if (sequence > _nextSequence.Get())
            {
                Debug.WriteLine(
                    "Sequence out of order, expected "
                        + _nextSequence.Get()
                        + " but received "
                        + sequence
                );
                // acknowledge that we receive this sequence but do not process it
                // until we're back in order
                Emit("outoforder", sequence);
                return false;
            }
            else
            {
                int ack = sequence;
                for (int i = 1; i < Constants.MAX_SEQUENCE; i++)
                {
                    // TODO: check if MAX_SEQUENCE + 1 is the right value
                    int fragmentIndex = (int)((_lastAck.Get() + i) & Constants.MAX_SEQUENCE);
                    if (_fragments.ContainsKey(fragmentIndex))
                    {
                        ack = fragmentIndex;
                    }
                    else
                    {
                        break;
                    }
                }
                _lastAck.Set(ack);
                // all sequences behind lastAck are acknowledged
                Emit("ack", ack);
                return true;
            }
        }

        public void write(byte[] data, int sequence, bool isFragment)
        {
            Debug.WriteLine(
                "Writing " + data.Length + " bytes, sequence " + sequence,
                " fragment=" + isFragment + ", lastAck: " + _lastAck.Get()
            );
            if (sequence >= _nextSequence.Get())
            {
                _fragments[sequence] = new Fragment { Payload = data, IsFragment = isFragment };
                bool wasInOrder = acknowledgeInputData(sequence);
                if (wasInOrder)
                {
                    _nextSequence.Set(_lastAck.Get() + 1);
                    _processData();
                }
            }
        }

        public void setEncryption(bool value)
        {
            _useEncryption = value;
            Debug.WriteLine("encryption: " + _useEncryption);
        }

        public void toggleEncryption()
        {
            _useEncryption = !_useEncryption;
            Debug.WriteLine("Toggling encryption: " + _useEncryption);
        }

        private (int length, int sizeValueBytes) ReadDataLength(byte[] data, int offset)
        {
            int length = data[offset];
            int sizeValueBytes;
            if (length == Constants.MAX_UINT8)
            {
                // if length is MAX_UINT8 then it's maybe a bigger number
                if (
                    data[offset + 1] == Constants.MAX_UINT8
                    && data[offset + 2] == Constants.MAX_UINT8
                )
                {
                    // it's an uint32
                    length = BitConverter.ToInt32(data, offset + 3);
                    sizeValueBytes = 7;
                }
                else
                {
                    // it's an uint16
                    length = BitConverter.ToInt16(data, offset + 1);
                    sizeValueBytes = 3;
                }
            }
            else
            {
                sizeValueBytes = 1;
            }
            return (length, sizeValueBytes);
        }

        private List<byte[]> ParseChannelPacketData(byte[] data)
        {
            var appData = new List<byte[]>();
            int offset,
                dataLength;
            if (data[0] == 0x00 && data[1] == 0x19)
            {
                // if it's a DataFragment packet
                offset = 2;
                while (offset < data.Length)
                {
                    (dataLength, int sizeValueBytes) = ReadDataLength(data, offset);
                    offset += sizeValueBytes;
                    byte[] temp = new byte[dataLength];
                    Array.Copy(data, offset, temp, 0, dataLength);
                    appData.Add(temp);
                    offset += dataLength;
                }
            }
            else
            {
                appData.Add(data);
            }
            return appData;
        }
    }
}
