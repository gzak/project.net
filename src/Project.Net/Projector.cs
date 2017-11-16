using System;
using System.Net.Http;

namespace Project.Net
{
    public class Projector : IProjector
    {
        private readonly HttpClient client;

        public Projector() { client = new HttpClient(); }

        public Projector(HttpClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            this.client = client;
        }

        public IProjection<T> Project<T>()
        {
            return new Projection<T>(client);
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }

    [SubDomain("goose")]
    [Plural("Geese")]
    public class Goose
    {
        public string X;
        public Goose M;
    }
}