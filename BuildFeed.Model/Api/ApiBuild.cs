using System;

namespace BuildFeed.Model.Api
{
    public class ApiBuild : BuildDetails
    {
        public Guid Id { get; set; }

        public string FullBuildString { get; set; }

        public string AlternateBuildString { get; set; }

        public string LabUrl { get; set; }

        public bool IsLeaked => SourceType == TypeOfSource.PublicRelease || SourceType == TypeOfSource.InternalLeak || SourceType == TypeOfSource.UpdateGDR;

        public DateTime Added { get; set; }

        public DateTime Modified { get; set; }
    }
}