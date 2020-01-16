using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ToDoApp
{
    /// <summary>
    /// Model of a collection of ToDo Categories and Items
    /// </summary>
    public sealed partial class MainPageData : INotifyPropertyChanged
    {
        private ObservableCollection<ToDoCategory> toDoCategories; // Collection of ToDo Categories and Items
        public ObservableCollection<ToDoCategory> ToDoCategories 
        {
            get => toDoCategories;
            set
            {
                toDoCategories = value;
                NotifyPropertyChanged();
            } 
        }

        private string titlebarText = "Unsaved File";
        [JsonIgnore]
        public string TitlebarText
        {
            get => titlebarText;
            set
            {
                if (titlebarText != value)
                {
                    titlebarText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private int widthSliderValue = 160;
        [JsonIgnore]
        public int WidthSliderValue
        {
            get => widthSliderValue;
            set
            {
                if (value != widthSliderValue)
                {
                    widthSliderValue = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string titlebarColor = "WhiteSmoke";
        [JsonIgnore]
        public string TitlebarColor
        {
            get => titlebarColor;
            set
            {
                if (value != titlebarColor)
                {
                    titlebarColor = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public MainPageData()
        {
            ToDoCategories = new ObservableCollection<ToDoCategory>();
        }
    }

    public sealed partial class MainPageDataSample : INotifyPropertyChanged
    {
        public ObservableCollection<string> TempList { get; set; }

        private ObservableCollection<ToDoCategory> toDoCategories; // Collection of ToDo Categories and Items
        public ObservableCollection<ToDoCategory> ToDoCategories
        {
            get => toDoCategories;
            set
            {
                toDoCategories = value;
                NotifyPropertyChanged();
            }
        }

        public string TitlebarText { get; set; } = "Temp Data";

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public MainPageDataSample()
        {
            ToDoCategories = new ObservableCollection<ToDoCategory>();
            ToDoCategories.Add(new ToDoCategory("Category 1"));
            ToDoCategories.Add(new ToDoCategory("Category 2"));
            ToDoCategories[0].ToDoItems.Add(new ToDoItem("Cat1 Item 1", "Blah blah blah", ItemType.Toggled));
            ToDoCategories[0].ToDoItems.Add(new ToDoItem("Cat1 Item 2", "Blah blah blah", ItemType.Toggled));
            ToDoCategories[0].ToDoItems.Add(new ToDoItem("Cat1 Item 3", "Blah blah blah", ItemType.Toggled));
            ToDoCategories[1].ToDoItems.Add(new ToDoItem("Cat2 Item 1", "Blah blah blah", ItemType.Toggled));
            ToDoCategories[1].ToDoItems.Add(new ToDoItem("Cat2 Item 2", "Blah blah blah", ItemType.Toggled));
            ToDoCategories[1].ToDoItems.Add(new ToDoItem("Cat2 Item 3", "Blah blah blah", ItemType.Toggled));

            TempList = new ObservableCollection<string>();
            TempList.Add("Item 1");
            TempList.Add("Item 2");
            TempList.Add("Item 3");
        }
    }
}