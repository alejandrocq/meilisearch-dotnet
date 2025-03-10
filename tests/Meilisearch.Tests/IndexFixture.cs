namespace Meilisearch.Tests
{
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class IndexFixture : IDisposable
    {
        public IndexFixture()
        {
            this.DefaultClient = new MeilisearchClient("http://localhost:7700", "masterKey");
        }

        public MeilisearchClient DefaultClient { get; private set; }

        public void Dispose()
        {
            this.DeleteAllIndexes().Wait(); // Let a clean MeiliSearch instance, for maintainers convenience only.
        }

        public async Task<Meilisearch.Index> SetUpBasicIndex(string indexUid)
        {
            Meilisearch.Index index = this.DefaultClient.Index(indexUid);
            var movies = new[]
            {
                new Movie { Id = "10", Name = "Gladiator" },
                new Movie { Id = "11", Name = "Interstellar" },
                new Movie { Id = "12", Name = "Star Wars", Genre = "SF" },
                new Movie { Id = "13", Name = "Harry Potter", Genre = "SF" },
                new Movie { Id = "14", Name = "Iron Man", Genre = "Action" },
                new Movie { Id = "15", Name = "Spider-Man", Genre = "Action" },
                new Movie { Id = "16", Name = "Amélie Poulain", Genre = "French movie" },
            };
            UpdateStatus update = await index.AddDocuments(movies);

            // Check the documents have been added
            UpdateStatus finalUpdateStatus = await index.WaitForPendingUpdate(update.UpdateId);
            if (finalUpdateStatus.Status != "processed")
            {
                throw new Exception("The documents were not added during SetUpBasicIndex. Impossible to run the tests.");
            }

            return index;
        }

        public async Task<Meilisearch.Index> SetUpBasicIndexWithIntId(string indexUid)
        {
            Meilisearch.Index index = this.DefaultClient.Index(indexUid);
            var movies = new[]
            {
                new MovieWithIntId { Id = 10, Name = "Gladiator" },
                new MovieWithIntId { Id = 11, Name = "Interstellar" },
                new MovieWithIntId { Id = 12, Name = "Star Wars", Genre = "SF" },
                new MovieWithIntId { Id = 13, Name = "Harry Potter", Genre = "SF" },
                new MovieWithIntId { Id = 14, Name = "Iron Man", Genre = "Action" },
                new MovieWithIntId { Id = 15, Name = "Spider-Man", Genre = "Action" },
                new MovieWithIntId { Id = 16, Name = "Amélie Poulain", Genre = "French movie" },
            };
            UpdateStatus update = await index.AddDocuments(movies);

            // Check the documents have been added
            UpdateStatus finalUpdateStatus = await index.WaitForPendingUpdate(update.UpdateId);
            if (finalUpdateStatus.Status != "processed")
            {
                throw new Exception("The documents were not added during SetUpBasicIndexWithIntId. Impossible to run the tests.");
            }

            return index;
        }

        public async Task<Meilisearch.Index> SetUpIndexForFaceting(string indexUid)
        {
            Meilisearch.Index index = this.DefaultClient.Index(indexUid);

            // Add documents
            var movies = new[]
            {
                new Movie { Id = "10", Name = "Gladiator" },
                new Movie { Id = "11", Name = "Interstellar" },
                new Movie { Id = "12", Name = "Star Wars", Genre = "SF" },
                new Movie { Id = "13", Name = "Harry Potter", Genre = "SF" },
                new Movie { Id = "14", Name = "Iron Man", Genre = "Action" },
                new Movie { Id = "15", Name = "Spider-Man", Genre = "Action" },
                new Movie { Id = "16", Name = "Amélie Poulain", Genre = "French movie" },
                new Movie { Id = "17", Name = "Mission Impossible", Genre = "Action" },
            };
            UpdateStatus update = await index.AddDocuments(movies);

            // Check the documents have been added
            UpdateStatus finalUpdateStatus = await index.WaitForPendingUpdate(update.UpdateId);
            if (finalUpdateStatus.Status != "processed")
            {
                throw new Exception("The documents were not added during SetUpIndexForFaceting. Impossible to run the tests.");
            }

            // Update settings
            Settings settings = new Settings
            {
                FilterableAttributes = new string[] { "genre" },
            };
            update = await index.UpdateSettings(settings);

            // Check the settings have been added
            finalUpdateStatus = await index.WaitForPendingUpdate(update.UpdateId);
            if (finalUpdateStatus.Status != "processed")
            {
                throw new Exception("The settings were not added during SetUpIndexForFaceting. Impossible to run the tests.");
            }

            return index;
        }

        public async Task DeleteAllIndexes()
        {
            var indexes = await this.DefaultClient.GetAllIndexes();
            foreach (var index in indexes)
            {
                await index.Delete();
            }
        }

        [CollectionDefinition("Sequential")]
        public class IndexCollection : ICollectionFixture<IndexFixture>
        {
            // This class has no code, and is never created. Its purpose is simply
            // to be the place to apply [CollectionDefinition] and all the
            // ICollectionFixture<> interfaces.

            // It makes the collections be executed sequentially because
            // the fixture and the tests are under the same collection named "Sequential"

            // Without using the fixture collection, this fixture would be called at the beginning of each
            // test class. We could control the execution order of the test classes but we could not control
            // the creation order of the fixture, which means the DeleteAllIndexes method would be called when
            // it's not expected.

            // cf https://xunit.net/docs/shared-context#collection-fixture
        }
    }
}
