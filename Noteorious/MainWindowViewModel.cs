namespace Noteorious
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    public class MainWindowViewModel
    {
        public MainWindowViewModel ()
        {
            NewTabCommand = new ActionCommand(p => NewTab());
            Tabs = new ObservableCollection<ITab>();
        }
        
        public ICommand NewTabCommand { get; }
        public ICollection<ITab> Tabs { get; }
        private void NewTab()
        {

        }
    }
    
}
