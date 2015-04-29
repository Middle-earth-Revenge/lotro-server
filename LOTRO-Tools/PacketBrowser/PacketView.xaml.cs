using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PacketBrowser
{
    /// <summary>
    /// Interaction logic for PacketView.xaml
    /// </summary>
    public partial class PacketView : UserControl
    {
        public PacketView()
        {
            InitializeComponent();
        }

        public PacketData Packet
        {
            get { return (PacketData)GetValue(PacketProperty); }
            set { SetValue(PacketProperty, value); }
        }

        public static readonly DependencyProperty PacketProperty = DependencyProperty.Register(
            "Packet",
            typeof(PacketData),
            typeof(PacketView),
            new FrameworkPropertyMetadata(OnPacketChanged));

        // Because of how intensive this control is, we only want to build the inner UI if the control is actually visible
        public bool IsInView
        {
            get { return m_IsInView; }
            set
            {
                if (m_IsInView != value)
                {
                    if (!m_IsInView && Packet != null && m_PacketChanged)
                    {
                        OnPacketAssigned();
                    }

                    m_IsInView = value;
                }
            }
        }
        private bool m_IsInView;

        private static void OnPacketChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PacketView view = (PacketView)d;

            view.m_PacketChanged = true;

            if (view.IsInView && e.NewValue != null)
                view.OnPacketAssigned();
        }

        private bool m_PacketChanged;

        // Re-initialize the UI with a new packet
        private void OnPacketAssigned()
        {
            m_PacketChanged = false;

            // Clear the old UI
            ByteValuesGrid.Children.Clear();
            ByteValuesGrid.ColumnDefinitions.Clear();
            ByteValuesGrid.RowDefinitions.Clear();
            RawDataGrid.Children.Clear();
            RawDataGrid.ColumnDefinitions.Clear();
            RawDataGrid.RowDefinitions.Clear();

            // Create the header row & column in the first grid
            ByteValuesGrid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(80)
            });
            ByteValuesGrid.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(20)
            });

            // Create 16 data columns in both grids
            for (int i = 0; i < 16; ++i)
            {
                ByteValuesGrid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(20)
                });

                RawDataGrid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(10)
                });
            }

            // Determine the number of data rows & create them
            int numRows = (Packet.RawData.Length / 16) + 1;
            for (int i = 0; i < numRows; ++i)
            {
                ByteValuesGrid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(20)
                });

                RawDataGrid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(20)
                });
            }

            // Fill in the first row (offsets)
            for (int i = 0; i < 16; ++i)
            {
                TextBlock label = new TextBlock()
                {
                    Text = i.ToString("X2"),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                };

                Border wrap = new Border()
                {
                    Background = new SolidColorBrush(Colors.LightGray)
                };
                wrap.Child = label;

                Grid.SetColumn(wrap, i + 1);
                Grid.SetRow(wrap, 0);

                ByteValuesGrid.Children.Add(wrap);
            }

            // Fill in the first column (begin address)
            for (int i = 0; i < numRows; ++i)
            {
                TextBlock label = new TextBlock()
                {
                    Text = (i * 16).ToString("X8"),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left
                };

                Border wrap = new Border()
                {
                    Background = new SolidColorBrush(Colors.LightGray)
                };
                wrap.Child = label;

                Grid.SetColumn(wrap, 0);
                Grid.SetRow(wrap, i + 1);

                ByteValuesGrid.Children.Add(wrap);
            }

            // Fill in the data
            for (int i = 0; i < Packet.RawData.Length; ++i)
            {
                TextBlock label = new TextBlock()
                {
                    Text = Packet.RawData[i].ToString("X2"),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                };
                Border wrap = new Border();
                wrap.Child = label;

                // The segment for this byte
                PacketSegment segment = Packet.GetSegmentForByte(i);
                if (segment != null)
                {
                    wrap.Background = new SolidColorBrush(segment.Color);
                }

                // Construct the tooltip
                string tooltip = string.Empty;
                tooltip += "byte: " + Packet.RawData[i];

                if (i <= Packet.RawData.Length - 2)
                {
                    // Can be a 16 bit value
                    tooltip += "\nint16: " + CustomBitConverter.ToUInt16(Packet.RawData, i);
                }

                if (i <= Packet.RawData.Length - 4)
                {
                    // Can be a 32 bit value
                    tooltip += "\nuint32: " + CustomBitConverter.ToUInt32(Packet.RawData, i);
                    tooltip += "\nfloat: " + CustomBitConverter.ToSingle(Packet.RawData, i);
                }

                if (i <= Packet.RawData.Length - ((int)Packet.RawData[i] + 1))
                {
                    // Try to convert to ASCII string of the given length
                    tooltip += "\nascii: " + System.Text.Encoding.ASCII.GetString(Packet.RawData, i + 1, (int)Packet.RawData[i]);
                }

                if (i <= Packet.RawData.Length - ((int)Packet.RawData[i] + 1) * 2)
                {
                    // Try to convert to unicode string of the given length
                    tooltip += "\nunicode: " + System.Text.Encoding.Unicode.GetString(Packet.RawData, i + 1, (int)Packet.RawData[i] * 2);
                }

                wrap.ToolTip = new TextBlock()
                {
                    Text = tooltip
                };

                // Handle visualization
                if (segment != null)
                {
                    wrap.MouseEnter += (s, e) =>
                    {
                        for (int j = 0; j < segment.Length; ++j)
                        {
                            Border b = RawDataGrid.Children[j + segment.Offset] as Border;
                            if (b != null)
                            {
                                b.Background = new SolidColorBrush(segment.Color);
                            }
                        }
                    };

                    wrap.MouseLeave += (s, e) =>
                    {
                        for (int j = 0; j < segment.Length; ++j)
                        {
                            Border b = RawDataGrid.Children[j + segment.Offset] as Border;
                            if (b != null)
                            {
                                b.Background = null;
                            }
                        }
                    };
                }

                Grid.SetColumn(wrap, (i % 16) + 1);
                Grid.SetRow(wrap, (i / 16) + 1);

                ByteValuesGrid.Children.Add(wrap);
            }

            // Fill in the ASCII representation
            for (int i = 0; i < Packet.RawData.Length; ++i)
            {
                char c = Convert.ToChar(Packet.RawData[i]);
                TextBlock label = new TextBlock()
                {
                    Text = c.ToString(),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                };

                Border wrap = new Border()
                {
                    //Background = new SolidColorBrush(Colors.Aqua)
                };
                wrap.Child = label;

                Grid.SetColumn(wrap, i % 16);
                Grid.SetRow(wrap, i / 16);

                RawDataGrid.Children.Add(wrap);
            }
        }

        private void CopyData_Click(object sender, RoutedEventArgs e)
        {
            // Copy the current packet's data into the clipboard in a format suitable for pasting into
            // popular hex editors
            if (Packet != null)
            {
                string data = string.Empty;
                foreach (byte b in Packet.RawData)
                {
                    data += b.ToString("X2") + " ";
                }
                data.TrimEnd();

                Clipboard.SetData(DataFormats.Text, data);
            }
        }
    }
}
