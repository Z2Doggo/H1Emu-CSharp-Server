namespace Servers.SOEOutputStream
{
    using H1EmuCore;
    using Servers.SOEInputStream;
    using System.Diagnostics;

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
            this.value = initValue;
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
            this.value = Wrap(this.value + value);
        }

        public void Set(int value)
        {
            this.value = Wrap(value);
        }

        public int Get()
        {
            return this.value;
        }

        public void Increment()
        {
            this.Add(1);
        }
    }

    public class SOEOutputStream : EventEmitter
    {
        private bool _UseEncryption = false;
        private int _FragmentSize = 0;
        private WrappedUint16 _Sequence = new WrappedUint16(-1);
        private WrappedUint16 _LastAck = new WrappedUint16(-1);

        // I don't know if this is the correct translation from Typescript to C# but I'll just roll with it I guess...
        Dictionary<int, CacheData> _Cache = new Dictionary<int, CacheData>();

        private RC4 _RC4;
        private bool _HadCacheError = false;
        void MyClass(byte[] cryptoKey)
        {
            _RC4 = new RC4(cryptoKey);
        }

        void AddToCache(int Sequence, byte[] Data, bool IsFragment)
        {
            _Cache[Sequence] = new CacheData { Data = Data, IsFragment = IsFragment, };
        }

        public class CacheData
        {
            public byte[]? Data;
            public bool IsFragment;
        }

        void RemoveFromCache(int sequence)
        {
            if (_Cache.ContainsKey(sequence))
            {
                _Cache.Remove(sequence);
            }
        }

        public void Write(byte[] data, bool unbuffered = false)
        {
            if (_UseEncryption)
            {
                data = _RC4.Encrypt(data);

                // if the first byte is a 0x00 then we need to add 1 more
                if (data[0] == 0)
                {
                    var tmp = new byte[1];
                    data = Concat(tmp, data);
                }
            }

            if (data.Length <= _FragmentSize)
            {
                _Sequence.Increment();
                AddToCache(_Sequence.Get(), data, false);
                Data(data, _Sequence.Get(), false, unbuffered);
            }
            else
            {
                var header = BitConverter.GetBytes(data.Length); // changed to a non-array version of GetBytes
                data = Concat(header, data);
                int fragmentCount = (int)Math.Ceiling(data.Length / (double)_FragmentSize); // added fragmentCount variable
                for (int i = 0; i < fragmentCount; i++) // changed loop to iterate over fragmentCount
                {
                    _Sequence.Increment();
                    int start = i * _FragmentSize;
                    int end = Math.Min(start + _FragmentSize, data.Length); // added end variable
                    var fragmentData = Slice(data, start, end); // changed to use Slice method
                    AddToCache(_Sequence.Get(), fragmentData, true);

                    Data(fragmentData, _Sequence.Get(), true, unbuffered);
                }
            }
        }

        public T[] Slice<T>(T[] array, int start, int end)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (start < 0 || start >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (end < 0 || end > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(end));
            }

            if (end < start)
            {
                throw new ArgumentException(
                    "End index must be greater than or equal to start index."
                );
            }

            int length = end - start;
            T[] result = new T[length];
            Array.Copy(array, start, result, 0, length);
            return result;
        }

        private static byte[] Concat(byte[] a, byte[] b)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(a, 0, a.Length);
                ms.Write(b, 0, b.Length);
                return ms.ToArray();
            }
        }

        private delegate void DataEventHandler(
            byte[] data,
            int sequence,
            bool isFragment,
            bool unbuffered
        );
        private event DataEventHandler Data = delegate { };

        void ResendData(int sequence)
        {
            var Emitter = new EventEmitter();
            if (_Cache.ContainsKey(sequence))
            {
                Emitter.Emit(
                    "dataResend",
                    _Cache[sequence].Data,
                    sequence,
                    _Cache[sequence].IsFragment
                );
            }
            else
            {
                // already deleted from cache so already acknowledged by the client not a real issue
                Debug.WriteLine($"Cache error, could not resend data for sequence {sequence}! ");
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

        public bool IsUsingEncryption()
        {
            return _UseEncryption;
        }

        public void SetEncryption(bool value)
        {
            _UseEncryption = value;
            Debug.WriteLine("encryption: " + _UseEncryption);
        }

        public void ToggleEncryption()
        {
            this._UseEncryption = !_UseEncryption;
            Debug.WriteLine("Toggling encryption: " + this._UseEncryption);
        }

        public void SetFragmentSize(int value)
        {
            _FragmentSize = value;
        }
        public SOEOutputStream(byte[] cryptoKey)
        {
            _UseEncryption = cryptoKey != null;
            _RC4 = _UseEncryption ? new RC4(cryptoKey) : null;
        }
    }
}