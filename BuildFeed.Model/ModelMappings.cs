using AutoMapper;
using BuildFeed.Model.Api;

namespace BuildFeed.Model
{
    public static class ModelMappings
    {
        public static void Initialise()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Build, ApiBuild>();
            });
        }
    }
}