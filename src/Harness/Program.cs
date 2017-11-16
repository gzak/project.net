using Project.Net;
using System.Threading.Tasks;

namespace Harness
{
    public class Program
    {
        private static readonly string arr1 = "hello";

        public class noxp
        {
            public noxp(string bloh) { }
            public IProjection<Goose> n;
            public IProjection<char> o;
            public string X;
            public int q;
        }

        public static void Main(string[] args)
        {
            HarnessIt().Wait();
        }

        private static async Task HarnessIt()
        {
            var arr2 = "world";
            using (IProjector proj = new Projector())
            {
                var x = from m in proj.Project<Goose>()
                        where m.X == null && m.M.X == arr1 && m.M.X == arr2
                        select new noxp(m.M.X)
                        {
                            n = from p in proj.Project<Goose>()
                                select p,
                            //o = from o in m.X.AsProjection()
                            //    from p in proj.Project<Goose>()
                            //    select o,
                            X = m.M.X,
                            //q = 5
                        };

                var y = await x;
            }
        }
    }
}