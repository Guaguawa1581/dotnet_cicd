namespace ApiServer.Infrastructure
{
    public static class LogIdGenerator
    {
        private static readonly object _lock = new();
        private static string _lastTimestamp = string.Empty;
        private static int _counter = 0;

        // 格式：yyyyMMddHHmmssfff（17碼）+ 3碼序號 = 20碼
        // 同一毫秒內序號 000–999；滿999則自旋等待下一毫秒
        public static string Next()
        {
            lock (_lock)
            {
                var ts = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                if (ts != _lastTimestamp)
                {
                    _lastTimestamp = ts;
                    _counter = 0;
                }
                else
                {
                    _counter++;
                    if (_counter > 999)
                    {
                        while ((ts = DateTime.Now.ToString("yyyyMMddHHmmssfff")) == _lastTimestamp)
                            Thread.SpinWait(100);
                        _lastTimestamp = ts;
                        _counter = 0;
                    }
                }

                return _lastTimestamp + _counter.ToString("D3");
            }
        }
    }
}
