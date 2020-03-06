using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Noteorious
{
    public class SpecialBox : RichTextBox
    {
        public ObservableCollection<TextRange> AdditionalRanges;
        
        public SpecialBox ()
        {
            
        }

        public void highlightSections ()
        {
            if (AdditionalRanges != null)
            {
                foreach (TextRange t in this.AdditionalRanges)
                {
                    t.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
                }
            }
                
        }

        public void dehighlightSections ()
        {
            if (AdditionalRanges != null)
            {
                foreach (TextRange t in this.AdditionalRanges)
                {
                    t.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
                }
            }
            
        }

    }
}
