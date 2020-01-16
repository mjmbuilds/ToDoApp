using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ToDoApp
{
    /// <summary>
    /// An individual item in a ToDoCategory
    /// </summary>
    public sealed class ToDoItem : INotifyPropertyChanged
    {
        private string itemName;        // name/breif description of item
        public string ItemName 
        {
            get => itemName;
            set 
            {
                if (value != null && value != itemName)
                {
                    itemName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string itemDescription; // optional long description of item
        public string ItemDescription 
        {
            get => itemDescription;
            set 
            {
                if (value != null && value != itemDescription)
                {
                    itemDescription = value;
                    NotifyPropertyChanged();
                }
            } 
        }

        private ItemType itemType;      // item type of (Once, Common, Toggled, Constant)
        public ItemType ItemType
        {
            get => itemType;
            set
            {
                if (value != itemType)
                {
                    itemType = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool itemComplete;      // status of task if ItemType of Common or Toggled
        public bool ItemComplete
        {
            get => itemComplete;
            set
            {
                if (value != itemComplete)
                {
                    itemComplete = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ToDoItem(string name, string description, ItemType type)
        {
            itemName = name;
            itemDescription = description;
            itemType = type;
            itemComplete = false;
        }
    }
}
