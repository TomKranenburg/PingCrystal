using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows;
using System.Windows.Controls;            // For context menu and UI controls
using System.Windows.Media;
using System.Windows.Input;               // For mouse input
using System.Windows.Media.Imaging;       // For image handling

namespace PingCrystal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// This class represents the main window of the application.
    /// </summary>
    public partial class MainWindow : Window
    {
        // Boolean variable to control the color-changing effect.
        // Default is false (effect is off).
        private bool colorChangingEnabled = false;

        /// <summary>
        /// Constructor for MainWindow.
        /// Initializes components, positions the window, and starts the ping checking thread.
        /// </summary>
        public MainWindow()
        {
            // Initialize UI components defined in XAML.
            InitializeComponent();

            // Set the window to always stay on top of other windows.
            this.Topmost = true;

            // Position the window at (10,10) pixels from the top-left corner of the primary screen.
            this.Left = 10;
            this.Top = 10;

            // Enable window dragging when the user clicks and holds the left mouse button.
            this.PreviewMouseLeftButtonDown += (s, e) => DragMove();

            // Start a new thread to run the PingCheck method continuously.
            Thread thread = new Thread(new ThreadStart(PingCheck));
            thread.Start();

            // Debug output to indicate that the MainWindow has been initialized.
            Trace.WriteLine("MainWindow initialized. PingCheck thread started.");
        }

        /// <summary>
        /// Method that continuously checks the network latency to 8.8.8.8.
        /// Updates the UI with the current ping time and adjusts text color based on latency.
        /// If the ping fails, displays "NA" and sets the window title to "NA".
        /// </summary>
        private void PingCheck()
        {
            while (true)
            {
                try
                {
                    // Create a new Ping object to send an ICMP echo request.
                    Ping myPing = new Ping();

                    // Send a ping to 8.8.8.8 with a timeout of 500 milliseconds.
                    PingReply reply = myPing.Send("8.8.8.8", 500);

                    // Ensure that UI updates are performed on the main thread.
                    this.Dispatcher.Invoke(() =>
                    {
                        if (reply.Status == IPStatus.Success)
                        {
                            // Ping was successful.
                            // Update the content of the 'Crystal' UI element with the round-trip time in milliseconds.
                            Crystal.Content = reply.RoundtripTime.ToString();

                            // Update the window title to include the current ping time.
                            Main_Window.Title = reply.RoundtripTime.ToString() + "ms";

                            // Check if color-changing effect is enabled.
                            if (colorChangingEnabled)
                            {
                                // Convert the round-trip time to an integer value.
                                int PingMs = unchecked((int)reply.RoundtripTime);

                                // Initialize variables for RGB color components.
                                byte RedAmount = 255;
                                byte GreenAmount = 255;
                                byte BlueAmount = 255;

                                // Calculate the decrease in Red and Green components based on ping time.
                                // We ensure that Red and Green do not drop below 128 to maintain brightness.
                                int colorDecrease = PingMs * 2;

                                // Ensure colorDecrease stays within valid range to prevent negative values.
                                if (colorDecrease > 127)
                                {
                                    colorDecrease = 127;
                                }

                                // Decrease Red and Green components while keeping Blue at maximum.
                                RedAmount = (byte)(255 - colorDecrease);
                                GreenAmount = (byte)(255 - colorDecrease);
                                BlueAmount = 255; // Blue remains at maximum to achieve bright blue.

                                // Debug output to show the ping time and calculated RGB values.
                                Trace.WriteLine($"Ping: {PingMs}ms, Color RGB: ({RedAmount}, {GreenAmount}, {BlueAmount})");

                                // Create a new SolidColorBrush with the calculated RGB color.
                                var brush = new SolidColorBrush(Color.FromRgb(RedAmount, GreenAmount, BlueAmount));

                                // Apply the brush to the foreground color of the 'Crystal' UI element.
                                Crystal.Foreground = brush;
                            }
                            else
                            {
                                // If the color-changing effect is disabled, ensure the text color is set to white.
                                Crystal.Foreground = new SolidColorBrush(Colors.White);
                            }
                        }
                        else
                        {
                            // Ping failed.
                            // Display "NA" in the 'Crystal' UI element.
                            Crystal.Content = "NA";

                            // Update the window title to "NA".
                            Main_Window.Title = "NA";

                            // Set the text color to white.
                            Crystal.Foreground = new SolidColorBrush(Colors.White);

                            // Debug output to indicate that the ping failed.
                            Trace.WriteLine($"Ping failed with status: {reply.Status}");
                        }
                    });

                    // Pause the thread for 1 second before the next ping.
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    // Debug output to indicate that an exception occurred during the ping operation.
                    Trace.WriteLine($"Error in PingCheck: {ex.Message}");

                    // Update the UI to indicate an error has occurred.
                    this.Dispatcher.Invoke(() =>
                    {
                        // Display "NA" in the 'Crystal' UI element.
                        Crystal.Content = "NA";

                        // Update the window title to "NA".
                        Main_Window.Title = "NA";

                        // Set the text color to white.
                        Crystal.Foreground = new SolidColorBrush(Colors.White);
                    });

                    // Wait for 1 second before retrying the ping operation.
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// Event handler when the 'Enable Color-Changing Effect' menu item is checked.
        /// Enables the color-changing effect.
        /// </summary>
        private void ColorChangingEffectChecked(object sender, RoutedEventArgs e)
        {
            // Enable the color-changing effect.
            colorChangingEnabled = true;

            // Debug output to indicate that the effect has been enabled.
            Trace.WriteLine("Color-Changing Effect enabled via context menu.");
        }

        /// <summary>
        /// Event handler when the 'Enable Color-Changing Effect' menu item is unchecked.
        /// Disables the color-changing effect.
        /// </summary>
        private void ColorChangingEffectUnchecked(object sender, RoutedEventArgs e)
        {
            // Disable the color-changing effect.
            colorChangingEnabled = false;

            // Reset the text color to default (White).
            this.Dispatcher.Invoke(() =>
            {
                Crystal.Foreground = new SolidColorBrush(Colors.White);
            });

            // Debug output to indicate that the effect has been disabled.
            Trace.WriteLine("Color-Changing Effect disabled via context menu.");
        }

        /// <summary>
        /// Event handler for the Exit menu item click event.
        /// Closes the application gracefully.
        /// </summary>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // Shutdown the current application.
            System.Windows.Application.Current.Shutdown();

            // Debug output to indicate that the application is shutting down.
            Trace.WriteLine("Application shutdown initiated by user.");
        }

        /// <summary>
        /// Event handler when the 'Always On Top' menu item is checked.
        /// Sets the window to always be on top.
        /// </summary>
        private void OnTopChecked(object sender, RoutedEventArgs e)
        {
            // Set the window to always stay on top.
            this.Topmost = true;

            // Debug output to indicate that 'Always On Top' has been enabled.
            Trace.WriteLine("Always on Top enabled via context menu.");
        }

        /// <summary>
        /// Event handler when the 'Always On Top' menu item is unchecked.
        /// Sets the window to not be always on top.
        /// </summary>
        private void OnTopUnchecked(object sender, RoutedEventArgs e)
        {
            // Set the window to not always stay on top.
            this.Topmost = false;

            // Debug output to indicate that 'Always On Top' has been disabled.
            Trace.WriteLine("Always on Top disabled via context menu.");
        }

        /// <summary>
        /// Event handler when the 'Crystal' menu item is checked.
        /// Sets the background image of 'Crystal' to a specified image with opacity.
        /// </summary>
        private void CrystalVisibleChecked(object sender, RoutedEventArgs e)
        {
            // Set the background image of the 'Crystal' button.
            var brush = new ImageBrush();

            // Set the image source to the specified image file.
            // Update the path to point to the correct location of your image file.
            brush.ImageSource = new BitmapImage(new Uri("E:\\Synced Storage\\Work\\PingCrystal\\Ping Counter Blue.png", UriKind.Absolute));

            // Set the opacity of the background image.
            brush.Opacity = 0.90;

            // Apply the brush as the background of the 'Crystal' button.
            Crystal.Background = brush;

            // Debug output to indicate that the background image has been set.
            Trace.WriteLine("Crystal background image set to 'Ping Counter Blue.png' with 90% opacity.");
        }

        /// <summary>
        /// Event handler when the 'Crystal' menu item is unchecked.
        /// Removes the background image of 'Crystal' by setting it to a blank image.
        /// </summary>
        private void CrystalVisibleUnchecked(object sender, RoutedEventArgs e)
        {
            // Remove the background image of the 'Crystal' button by setting it to a blank image.
            var brush = new ImageBrush();

            // Set the image source to a blank image file.
            // Update the path to point to the correct location of your blank image file.
            brush.ImageSource = new BitmapImage(new Uri("E:\\Synced Storage\\Work\\PingCrystal\\Blank.png", UriKind.Absolute));

            // Apply the brush as the background of the 'Crystal' button.
            Crystal.Background = brush;

            // Debug output to indicate that the background image has been removed.
            Trace.WriteLine("Crystal background image set to 'Blank.png'.");
        }
    }
}
