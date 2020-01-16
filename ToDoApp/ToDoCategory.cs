using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ToDoApp
{
    /// <summary>
    /// A collection of ToDoItem items
    /// </summary>
    public sealed class ToDoCategory : INotifyPropertyChanged
    {
        private string categoryName;     // name of this Category
        public string CategoryName 
        { 
            get => categoryName;
            set 
            {
                if (value != null && value != categoryName)
                {
                    categoryName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ItemType defaultItemType; // item type of (Once, Common, Toggled, Constant)
        public ItemType DefaultItemType 
        {
            get => defaultItemType;
            set
            {
                defaultItemType = value;
                NotifyPropertyChanged();
            }
        }

        private int width;              // desired width of this Category column
        public int Width 
        {
            get => width;
            set 
            {
                if (value != this.width)
                {
                    this.width = value;
                    NotifyPropertyChanged();
                }
            } 
        }

        public ObservableCollection<ToDoItem> ToDoItems { get; set; }   // list of ToDo items belonging to this Category

        public ObservableCollection<ToDoItem> CommonItems { get; set; } // list of Common completed ToDo items belonging to this Category

        [JsonIgnore]
        public ObservableCollection<ToDoItem> Recycling { get; set; }   // list of Recycled ToDo items belonging to this Category

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ToDoCategory(string name, int categoryWidth = 160, ItemType itemType = ItemType.Common)
        {
            CategoryName = name;
            ToDoItems = new ObservableCollection<ToDoItem>();
            CommonItems = new ObservableCollection<ToDoItem>();
            Recycling = new ObservableCollection<ToDoItem>();
            defaultItemType = itemType;
            width = categoryWidth;
        }   
    }
}
