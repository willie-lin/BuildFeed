using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using BuildFeed.Model.View;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BuildFeed.Model
{
    public partial class BuildRepository
    {
        private const string BUILD_COLLECTION_NAME = "builds";
        private static readonly BsonDocument sortByAddedDate = new BsonDocument(nameof(Build.Added), -1);
        private static readonly BsonDocument sortByCompileDate = new BsonDocument(nameof(Build.BuildTime), -1);
        private static readonly BsonDocument sortByLeakedDate = new BsonDocument(nameof(Build.LeakDate), -1);

        private static readonly BsonDocument sortByOrder = new BsonDocument
        {
            new BsonElement(nameof(Build.MajorVersion), -1),
            new BsonElement(nameof(Build.MinorVersion), -1),
            new BsonElement(nameof(Build.Number), -1),
            new BsonElement(nameof(Build.Revision), -1),
            new BsonElement(nameof(Build.BuildTime), -1)
        };

        private readonly IMongoCollection<Build> _buildCollection;

        public BuildRepository()
        {
            MongoClientSettings settings = new MongoClientSettings
            {
                Server = new MongoServerAddress(MongoConfig.Host, MongoConfig.Port)
            };

            if (!string.IsNullOrEmpty(MongoConfig.Username) && !string.IsNullOrEmpty(MongoConfig.Password))
            {
                settings.Credentials = new List<MongoCredential>
                {
                    MongoCredential.CreateCredential(MongoConfig.Database, MongoConfig.Username, MongoConfig.Password)
                };
            }

            MongoClient dbClient = new MongoClient(settings);

            IMongoDatabase buildDatabase = dbClient.GetDatabase(MongoConfig.Database);
            _buildCollection = buildDatabase.GetCollection<Build>(BUILD_COLLECTION_NAME);
        }

        public async Task SetupIndexes()
        {
            List<BsonDocument> indexes = await (await _buildCollection.Indexes.ListAsync()).ToListAsync();

            if (indexes.All(i => i["name"] != "_idx_group"))
            {
                await
                    _buildCollection.Indexes.CreateOneAsync(
                        Builders<Build>.IndexKeys.Combine(Builders<Build>.IndexKeys.Descending(b => b.MajorVersion),
                            Builders<Build>.IndexKeys.Descending(b => b.MinorVersion),
                            Builders<Build>.IndexKeys.Descending(b => b.Number),
                            Builders<Build>.IndexKeys.Descending(b => b.Revision)),
                        new CreateIndexOptions
                        {
                            Name = "_idx_group"
                        });
            }

            if (indexes.All(i => i["name"] != "_idx_legacy"))
            {
                await _buildCollection.Indexes.CreateOneAsync(Builders<Build>.IndexKeys.Ascending(b => b.LegacyId),
                    new CreateIndexOptions
                    {
                        Name = "_idx_legacy"
                    });
            }

            if (indexes.All(i => i["name"] != "_idx_lab"))
            {
                await _buildCollection.Indexes.CreateOneAsync(Builders<Build>.IndexKeys.Ascending(b => b.Lab),
                    new CreateIndexOptions
                    {
                        Name = "_idx_lab"
                    });
            }

            if (indexes.All(i => i["name"] != "_idx_date"))
            {
                await _buildCollection.Indexes.CreateOneAsync(Builders<Build>.IndexKeys.Descending(b => b.BuildTime),
                    new CreateIndexOptions
                    {
                        Name = "_idx_date"
                    });
            }

            if (indexes.All(i => i["name"] != "_idx_bstr"))
            {
                await _buildCollection.Indexes.CreateOneAsync(Builders<Build>.IndexKeys.Ascending(b => b.FullBuildString),
                    new CreateIndexOptions
                    {
                        Name = "_idx_bstr"
                    });
            }

            if (indexes.All(i => i["name"] != "_idx_alt_bstr"))
            {
                await _buildCollection.Indexes.CreateOneAsync(Builders<Build>.IndexKeys.Ascending(b => b.AlternateBuildString),
                    new CreateIndexOptions
                    {
                        Name = "_idx_alt_bstr"
                    });
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public async Task<List<Build>> Select() => await _buildCollection.Find(new BsonDocument()).ToListAsync();

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public async Task<Build> SelectById(Guid id) => await _buildCollection.Find(Builders<Build>.Filter.Eq(b => b.Id, id)).SingleOrDefaultAsync();

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public async Task<Build> SelectByLegacyId(long id) => await _buildCollection.Find(Builders<Build>.Filter.Eq(b => b.LegacyId, id)).SingleOrDefaultAsync();

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public async Task<List<Build>> SelectBuildsByOrder(int limit = -1, int skip = 0)
        {
            IFindFluent<Build, Build> query = _buildCollection.Find(new BsonDocument()).Sort(sortByOrder).Skip(skip);

            if (limit > 0)
            {
                query = query.Limit(limit);
            }

            return await query.ToListAsync();
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public async Task<FrontPage> SelectFrontPage()
        {
            FrontPage fp = new FrontPage();

            IFindFluent<Build, Build> query = _buildCollection.Find(new BsonDocument
            {
                {
                    nameof(Build.LabUrl), new BsonDocument
                    {
                        {"$in", new BsonArray(ConfigurationManager.AppSettings["site:OSGLab"].Split(';'))}
                    }
                }
            }).Sort(sortByCompileDate).Limit(1);
            fp.CurrentCanary = await query.FirstOrDefaultAsync();

            query = _buildCollection.Find(new BsonDocument
            {
                {
                    nameof(Build.LabUrl), new BsonDocument
                    {
                        {"$in", new BsonArray(ConfigurationManager.AppSettings["site:InsiderLab"].Split(';'))}
                    }
                },
                {
                    nameof(Build.SourceType), new BsonDocument
                    {
                        {
                            "$in", new BsonArray
                            {
                                TypeOfSource.PublicRelease,
                                TypeOfSource.UpdateGDR
                            }
                        }
                    }
                }
            }).Sort(sortByCompileDate).Limit(1);
            fp.CurrentInsider = await query.FirstOrDefaultAsync();

            query = _buildCollection.Find(new BsonDocument
            {
                {
                    nameof(Build.LabUrl), new BsonDocument
                    {
                        {"$in", new BsonArray(ConfigurationManager.AppSettings["site:ReleaseLab"].Split(';'))}
                    }
                },
                {
                    nameof(Build.SourceType), new BsonDocument
                    {
                        {
                            "$in", new BsonArray
                            {
                                TypeOfSource.PublicRelease,
                                TypeOfSource.UpdateGDR
                            }
                        }
                    }
                }
            }).Sort(sortByCompileDate).Limit(1);
            fp.CurrentRelease = await query.FirstOrDefaultAsync();


            query = _buildCollection.Find(new BsonDocument
            {
                {
                    nameof(Build.LabUrl), new BsonDocument
                    {
                        {"$in", new BsonArray(ConfigurationManager.AppSettings["site:XboxLab"].Split(';'))}
                    }
                }
            }).Sort(sortByCompileDate).Limit(1);
            fp.CurrentXbox = await query.FirstOrDefaultAsync();

            return fp;
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public async Task<List<Build>> SelectBuildsByStringSearch(string term, int limit = -1)
        {
            IAggregateFluent<Build> query = _buildCollection.Aggregate().Match(b => b.FullBuildString != null).Match(b => b.FullBuildString != "").Match(b => b.FullBuildString.ToLower().Contains(term.ToLower()));

            if (limit > 0)
            {
                query = query.Limit(limit);
            }

            return await query.ToListAsync();
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public async Task<Build> SelectBuildByFullBuildString(string build)
        {
            return await _buildCollection.Find(Builders<Build>.Filter.Eq(b => b.FullBuildString, build)).SingleOrDefaultAsync();
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public async Task<List<Build>> SelectBuildsByCompileDate(int limit = -1, int skip = 0)
        {
            IFindFluent<Build, Build> query = _buildCollection.Find(new BsonDocument()).Sort(sortByCompileDate).Skip(skip);

            if (limit > 0)
            {
                query = query.Limit(limit);
            }

            return await query.ToListAsync();
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public async Task<List<Build>> SelectBuildsByAddedDate(int limit = -1, int skip = 0)
        {
            IFindFluent<Build, Build> query = _buildCollection.Find(new BsonDocument()).Sort(sortByAddedDate).Skip(skip);

            if (limit > 0)
            {
                query = query.Limit(limit);
            }

            return await query.ToListAsync();
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public async Task<List<Build>> SelectBuildsByLeakedDate(int limit = -1, int skip = 0)
        {
            IFindFluent<Build, Build> query = _buildCollection.Find(new BsonDocument()).Sort(sortByLeakedDate).Skip(skip);

            if (limit > 0)
            {
                query = query.Limit(limit);
            }

            return await query.ToListAsync();
        }

        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public async Task Insert(Build item)
        {
            item.Id = Guid.NewGuid();
            item.RegenerateCachedProperties();

            await _buildCollection.InsertOneAsync(item);
        }

        [DataObjectMethod(DataObjectMethodType.Insert, false)]
        public async Task InsertAll(IEnumerable<Build> items)
        {
            var generatedItems = new List<Build>();
            foreach (Build item in items)
            {
                item.Id = Guid.NewGuid();
                item.RegenerateCachedProperties();

                generatedItems.Add(item);
            }

            await _buildCollection.InsertManyAsync(generatedItems);
        }

        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public async Task Update(Build item)
        {
            Build old = await SelectById(item.Id);
            item.Added = old.Added;
            item.Modified = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            item.RegenerateCachedProperties();

            await _buildCollection.ReplaceOneAsync(Builders<Build>.Filter.Eq(b => b.Id, item.Id), item);
        }

        [DataObjectMethod(DataObjectMethodType.Delete, true)]
        public async Task DeleteById(Guid id)
        {
            await _buildCollection.DeleteOneAsync(Builders<Build>.Filter.Eq(b => b.Id, id));
        }

        public async Task MigrateAddedModifiedToHistory()
        {
            List<Build> builds = await Select();
            foreach (Build bd in builds)
            {
                BuildDetails item = new BuildDetails
                {
                    MajorVersion = bd.MajorVersion,
                    MinorVersion = bd.MinorVersion,
                    Number = bd.Number,
                    Revision = bd.Revision,
                    Lab = bd.Lab,
                    BuildTime = bd.BuildTime,
                    SourceType = bd.SourceType,
                    LeakDate = bd.LeakDate,
                    SourceDetails = bd.SourceDetails
                };

                if (bd.Added == DateTime.MinValue)
                {
                    continue;
                }

                bd.History = new List<ItemHistory<BuildDetails>>
                {
                    new ItemHistory<BuildDetails>
                    {
                        Type = ItemHistoryType.Added,
                        Time = bd.Added,
                        UserName = "",
                        Item = bd.Added == bd.Modified
                            ? item
                            : null
                    }
                };

                if (bd.Modified != DateTime.MinValue && bd.Added != bd.Modified)
                {
                    bd.History.Add(new ItemHistory<BuildDetails>
                    {
                        Type = ItemHistoryType.Edited,
                        Time = bd.Modified,
                        UserName = "",
                        Item = item
                    });
                }

                await _buildCollection.ReplaceOneAsync(Builders<Build>.Filter.Eq(b => b.Id, bd.Id), bd);
            }
        }

        public async Task RegenerateCachedProperties()
        {
            List<Build> builds = await Select();
            foreach (Build bd in builds)
            {
                bd.RegenerateCachedProperties();
                await _buildCollection.ReplaceOneAsync(Builders<Build>.Filter.Eq(b => b.Id, bd.Id), bd);
            }
        }
    }
}