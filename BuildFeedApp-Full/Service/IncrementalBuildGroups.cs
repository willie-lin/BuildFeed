using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace BuildFeedApp.Service
{
   public class IncrementalBuildGroups : List<FrontBuildGroup>, ISupportIncrementalLoading, INotifyCollectionChanged
   {
      private bool _hasMoreItems;
      private bool _busy;

      public bool HasMoreItems => _hasMoreItems;

      public event NotifyCollectionChangedEventHandler CollectionChanged;

      public IncrementalBuildGroups() : base()
      {
         _hasMoreItems = true;
         _busy = false;
      }

      private void NotifyOfInsertedItems(FrontBuildGroup[] newItems)
      {
         if (CollectionChanged == null) return;

         foreach(var item in newItems)
         {
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, IndexOf(item));
            CollectionChanged(this, args);
         }
      }

      private async Task<LoadMoreItemsResult> _loadMoreItemsAsync(CancellationToken c, uint count)
      {
         try
         {
            FrontBuildGroup[] getBuilds = await ApiCache.GetApi<FrontBuildGroup[]>($"https://buildfeed.net/api/GetBuildGroups?skip={this.Count}&limit={count}");
            AddRange(getBuilds);
            NotifyOfInsertedItems(getBuilds);

            _hasMoreItems = getBuilds.Length == count;
            return new LoadMoreItemsResult()
            {
               Count = (uint)getBuilds.Length
            };
         }
         finally
         {
            _busy = false;
         }
      }

      public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
      {
         if (_busy)
         {
            throw new InvalidOperationException("Only one operation in flight at a time");
         }

         _busy = true;

         return AsyncInfo.Run((c) => _loadMoreItemsAsync(c, count));
      }
   }
}
