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
}