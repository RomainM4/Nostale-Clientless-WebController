//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NosCore.Shared.Helpers
{
    public class RandomHelper : IDisposable
    {
        private static RandomHelper? _instance;
        private static int _seed = Environment.TickCount;

        private readonly ThreadLocal<Random> _random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        private RandomHelper()
        {
        }

        public static RandomHelper Instance => _instance ??= new RandomHelper();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public int RandomNumber()
        {
            return RandomNumber(0, 100);
        }

        public int RandomNumber(int min, int max)
        {
            return _random.Value!.Next(min, max);
        }

        protected virtual void Dispose(bool disposing)
        {
            _random.Dispose();
        }
    }
}
