using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace BuildFeed.Models
{
   public partial class Build
   {

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectLab(string lab, int skip, int limit)
      {
         string labUrl = lab.Replace('/', '-').ToLower();
         return await _buildCollection.Find(b => b.Lab != null && b.LabUrl == labUrl)
             .SortByDescending(b => b.BuildTime)
             .ThenByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .Skip(skip)
             .Limit(limit)
             .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<string>> SelectLabs()
      {
         var result = await _buildCollection.Aggregate()
            .Match(b => b.Lab != null)
            .Match(b => b.Lab != "")
            .Group(b => b.Lab.ToLower(),
            // incoming bullshit hack
            bg => new Tuple<string>(bg.Key))
            .SortBy(b => b.Item1)
            .ToListAsync();

         // work ourselves out of aforementioned bullshit hack
         return result.Select(b => b.Item1).ToList();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<string>> SelectLabs(byte major, byte minor)
      {
         var result = await _buildCollection.Aggregate()
            .Match(b => b.MajorVersion == major)
            .Match(b => b.MinorVersion == minor)
            .Match(b => b.Lab != null)
            .Match(b => b.Lab != "")
            .Group(b => b.Lab.ToLower(),
            // incoming bullshit hack
            bg => new Tuple<string>(bg.Key))
            .SortBy(b => b.Item1)
            .ToListAsync();

         // work ourselves out of aforementioned bullshit hack
         return result.Select(b => b.Item1).ToList();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<long> SelectLabCount(string lab)
      {
         string labUrl = lab.Replace('/', '-').ToLower();
         return await _buildCollection.Find(b => b.Lab != null && b.LabUrl == labUrl)
            .CountAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<string>> SearchBuildLabs(string query)
      {
         var result = await _buildCollection.Aggregate()
            .Match(b => b.Lab != null)
            .Match(b => b.Lab != "")
            .Match(b => b.Lab.ToLower().Contains(query.ToLower()))
            .Group(b => b.Lab.ToLower(),
            // incoming bullshit hack
            bg => new Tuple<string>(bg.Key))
            .ToListAsync();

         // work ourselves out of aforementioned bullshit hack
         return result.Select(b => b.Item1).ToList();
      }
   }
}