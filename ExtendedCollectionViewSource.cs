using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SeveQsDataBase
{
    public class ExtendedCollectionViewSource : CollectionViewSource
    {
        private BindingListAdapter mAdapter;

        static ExtendedCollectionViewSource()
        {
            SourceProperty.OverrideMetadata(
                typeof(ExtendedCollectionViewSource), new FrameworkPropertyMetadata(null, CoerceSource));
        }

        static object CoerceSource(DependencyObject d, object baseValue)
        {
            ExtendedCollectionViewSource cvs = (ExtendedCollectionViewSource)d;
            if (cvs.mAdapter != null)
            {
                cvs.mAdapter.Dispose();
                cvs.mAdapter = null;
            }

            IBindingList bindingList = baseValue as IBindingList;
            if (bindingList != null)
            {
                cvs.mAdapter = new BindingListAdapter(bindingList);
                return cvs.mAdapter;
            }
            return baseValue;
        }
    }

    internal class BindingListAdapter : ObservableCollection<object>, IDisposable
    {
        private IBindingList mBindingList;
        private bool mIsDisposed;

        public BindingListAdapter(IBindingList bindingList)
        {
            if (bindingList == null)
            {
                throw new ArgumentNullException("bindingList");
            }

            mBindingList = bindingList;

            foreach (object item in mBindingList)
            {
                Items.Add(item);
            }

            mBindingList.ListChanged += BindingList_Changed;
        }

        private void BindingList_Changed(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    {
                        InsertItem(e.NewIndex, mBindingList[e.NewIndex]);
                        break;
                    }
                case ListChangedType.ItemChanged:
                    {
                        SetItem(e.NewIndex, mBindingList[e.NewIndex]);
                        break;
                    }
                case ListChangedType.ItemDeleted:
                    {
                        RemoveItem(e.NewIndex);
                        break;
                    }
                case ListChangedType.ItemMoved:
                    {
                        MoveItem(e.OldIndex, e.NewIndex);
                        break;
                    }
                case ListChangedType.Reset:
                    {
                        Items.Clear();
                        foreach (object item in mBindingList)
                        {
                            Items.Add(item);
                        }

                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                        break;
                    }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!mIsDisposed)
            {
                if(disposing)
                {
                    mBindingList.ListChanged -= BindingList_Changed;
                }
            }
            mIsDisposed = true;
        }
    }
}
