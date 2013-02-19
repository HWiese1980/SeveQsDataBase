using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SeveQsDataBase
{
    using System.IO;
    using System.Reflection;
    using System.Text;

    public abstract class DataBase : INotifyPropertyChanged, IHasParent, IIndexed, IOnPropertyChanged
    {
        [ObserveProperty]
        public static bool IsDirty
        {
            get
            {
                return isDirty;
            }
            set
            {
                isDirty = value;
            }
        }

        protected static void CleanMe(DataBase me)
        {
            IsDirty = false;
            me.OnPropertyChanged("IsDirty");
        }

        protected static void DirtyMe(DataBase me)
        {
            IsDirty = true;
            me.OnPropertyChanged("IsDirty");
        }

        public static event EventHandler<ValueEventArgs<string>> StatusChanged;
        protected void OnStatusChanged(string format, params object[] data)
        {
            if (StatusChanged != null) StatusChanged(this, new ValueEventArgs<string>(String.Format(format, data)));
        }

        public void Refresh()
        {
            var properties = GetType().GetProperties();
            foreach (var prop in properties) OnPropertyChanged(prop.Name);
        }

        public int Index
        {
            get
            {
                return this.index;
            }
            set
            {
                this.index = value;
                // this.OnPropertyChanged();
            }
        }

        protected ICollectionView View { get; set; }

        public IEnumerable<IIndexed> IndexGroup { get; set; }


        public static DataBase DB { get; set; }

        private readonly List<DataBase> _otherAncestors = new List<DataBase>();
        public IList<DataBase> OtherAncestors
        {
            get { return _otherAncestors; }
        }

        public DataBase Parent { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        public abstract string Name { get; }

        /// <summary>
        /// Gets or sets a value indicating whether is loading.
        /// </summary>
        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }
            set
            {
                this.isLoading = value;
                OnPropertyChanged();
            }
        }

        protected Guid _id;

        private int index;

        protected DataBase()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (sender, args) => Assembly.ReflectionOnlyLoad(args.Name);
            _id = Guid.NewGuid();
        }

        ~DataBase()
        {
            PropertyChanged = null;
        }

        private static readonly Dictionary<string, List<IOnPropertyChanged>> _otherObjectsToNotify = new Dictionary<string, List<IOnPropertyChanged>>();

        public static void RegisterNotifiedObject(IOnPropertyChanged obj, string property)
        {
            if (!_otherObjectsToNotify.ContainsKey(property)) _otherObjectsToNotify[property] = new List<IOnPropertyChanged>();
            _otherObjectsToNotify[property].Add(obj);
        }

        private static StreamWriter sr = new StreamWriter(new FileStream(@"V:\taxitaxi.log", FileMode.Create, FileAccess.Write, FileShare.Write), Encoding.UTF8) { AutoFlush = true, NewLine = "\n" };

        private void Log(string format, params object[] data)
        {
            lock (sr)
            {
                sr.WriteLine(format, data);
            }
        }

        private bool bInitializing = true;
        public void EndInitialize()
        {
            bInitializing = false;
        }

        private static object opclock = new object();

        private static bool isDirty;


        /* TODO: Caching Funktion
         * Ein globales Dictionary, das als Key den Propertypath zusammen mit einer Objekt-GUID hat und als Value den neuen Wert beim Aufruf von 
         * OnPropertyChanged bekommt. 
         * Der Getter einer Property prüft erst die Existenz des entsprechenden Keys und nutzt bei Vorhandensein den Wert aus dem Dictionary. 
         * Andernfalls wird der Wert neu berechnet und in das Caching Dictionary eingetragen. */

        private Dictionary<string, IEnumerable<PropertyInfo>> _propInfos = new Dictionary<string, IEnumerable<PropertyInfo>>();

        /// <summary>
        /// The is loading.
        /// </summary>
        private bool isLoading;

#if USE_SEVEQ_DB
        public async virtual Task<string> OnPropertyChanged([CallerMemberName]string property = "", string by = "", int recursionLevel = 0)
        {
            // lock (opclock)
            {
                if (by == "")
                {
                    by = "Application";
                }

                if (ObservePropertyAttribute.Logging) Log("[OPC:{0} by {1}|{2}]".Indent(recursionLevel), property, by, Name);

                if (PropertyChanged != null && !property.Contains("."))
                {
                    if (ObservePropertyAttribute.Logging) Log("[T:{0}|{1}|{2}]".Indent(recursionLevel), property, Name, recursionLevel);
                    PropertyChanged(this, new PropertyChangedEventArgs(property));
                }
                if (property == by) return property;

                string tag = GetType().Name + "." + property;
                string pInf = String.Format("{0}|{1}|{2}", tag, by, recursionLevel);
                if (!_propInfos.ContainsKey(pInf))
                {
                    var ownDependingProperties = (from p in GetType().GetProperties()
                                                  let attrib = p.GetCustomAttributes<ObservePropertyAttribute>()
                                                  where attrib.Any(q => q.Dependency == property || q.Dependency == "*")
                                                  select p).Distinct(new PropertyInfoComparer()).ToArray();
                    _propInfos[pInf] = ownDependingProperties;
                }

                foreach (var ownProperty in _propInfos[pInf])
                {
                    if (ObservePropertyAttribute.Logging) Log("[D:{0}|{1}|{2}]".Indent(recursionLevel), ownProperty.Name, Name, recursionLevel);
                    if (IsLoading && ownProperty.GetCustomAttribute<IsGlobalPropertyAttribute>() != null)
                    {
                        continue;
                    }
                    await OnPropertyChanged(ownProperty.Name, property, recursionLevel + 1);
                }

                if (Parent != null)
                {
                    if (ObservePropertyAttribute.Logging) Log("[P:{0}|{1}]".Indent(recursionLevel), Parent.Name, recursionLevel);
                    await Parent.OnPropertyChanged(tag, property, recursionLevel + 1);
                }
                else
                {
                    this.Log("[W:poor {0} has got no parents".Indent(recursionLevel), Name);
                }
                if (DB != null && !property.StartsWith("DB.")) DB.OnPropertyChanged(tag, property, recursionLevel + 1);

                if (_otherObjectsToNotify.ContainsKey(tag))
                {
                    foreach (var otherObject in _otherObjectsToNotify[tag])
                    {
                        await otherObject.OnPropertyChanged(tag, property, recursionLevel + 1);
                    }
                }

                return property;
            }
        }
#else
        public void OnPropertyChanged([CallerMemberName]string property = "", string by = "")
        {
            if(PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
            var ownDependingProperties = (from p in GetType().GetProperties()
                                          let attrib = p.GetCustomAttributes(typeof(ObservePropertyAttribute), true).Cast<ObservePropertyAttribute>().ToList()
                                          where attrib.Any(q => q.Dependency == property || q.Dependency == "*")
                                          select p).Distinct(new PropertyInfoComparer()).Select(p => p.Name).ToArray();
            foreach(var o in ownDependingProperties)
            {
                OnPropertyChanged(o);
            }
        }
#endif
        public override string ToString()
        {
            return string.Format("[{0} - {1} - {2}]", GetType().Name, _id, Name);
        }

    }
}