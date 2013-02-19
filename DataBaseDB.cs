// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataBaseDB.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the DataBaseDB type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Reflection;

namespace SeveQsDataBase
{
    using System;
    using System.Linq;
    using System.Windows.Threading;

    /// <summary>
    /// The data base db.
    /// </summary>
    public abstract class DataBaseDB : DataBase
    {
        /// <summary>
        /// Gets or sets the ui dispatcher.
        /// </summary>
        public Dispatcher UIDispatcher { get; set; }

        protected void OnFileLoaded(string File)
        {
            foreach(var globalProp in GetType().GetProperties().Where(p => p.GetCustomAttribute<IsGlobalPropertyAttribute>() != null))
            {
                OnPropertyChanged(globalProp.Name);
            }

            if (FileLoaded != null)
            {
                FileLoaded(this, new ValueEventArgs<string>(File));
            }
        }

        public abstract void Save(string txiFile);
        public event EventHandler<ValueEventArgs<string>> FileLoaded;
        public abstract void Load(string txiFile);
    }
}
