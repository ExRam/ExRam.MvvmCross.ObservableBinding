namespace ExRam.MvvmCross.ObservableBinding
{
    public sealed class Boxed<T> where T : struct
    {
        private readonly T _value;

        public Boxed(T value)
        {
            this._value = value;
        }

        public T Value
        {
            get
            {
                return this._value;
            }
        }
    }
}
