#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// SizeObserver.cs
//
// This file is part of JumpForJoy Software's WPFUtilities.
// 
// WPFUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// WPFUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with WPFUtilities. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Windows;

namespace J4JSoftware.WPFUtilities;

// thanx to Ken Boogaart & Athari for this!
// https://stackoverflow.com/questions/1083224/pushing-read-only-gui-properties-back-into-viewmodel/1083733#1083733
public static class SizeObserver
{
    public static readonly DependencyProperty ObserveProperty = DependencyProperty.RegisterAttached( "Observe",
        typeof( bool ),
        typeof( SizeObserver ),
        new FrameworkPropertyMetadata( OnObserveChanged ) );

    public static readonly DependencyProperty ObservedWidthProperty =
        DependencyProperty.RegisterAttached( "ObservedWidth",
                                             typeof( double ),
                                             typeof( SizeObserver ) );

    public static readonly DependencyProperty ObservedHeightProperty =
        DependencyProperty.RegisterAttached( "ObservedHeight",
                                             typeof( double ),
                                             typeof( SizeObserver ) );

    public static bool GetObserve( FrameworkElement frameworkElement ) =>
        (bool) frameworkElement.GetValue( ObserveProperty );

    public static void SetObserve( FrameworkElement frameworkElement, bool observe ) =>
        frameworkElement.SetValue( ObserveProperty, observe );

    public static double GetObservedWidth( FrameworkElement frameworkElement ) =>
        (double) frameworkElement.GetValue( ObservedWidthProperty );

    public static void SetObservedWidth( FrameworkElement frameworkElement, double observedWidth ) =>
        frameworkElement.SetValue( ObservedWidthProperty, observedWidth );

    public static double GetObservedHeight( FrameworkElement frameworkElement ) =>
        (double) frameworkElement.GetValue( ObservedHeightProperty );

    public static void SetObservedHeight( FrameworkElement frameworkElement, double observedHeight ) =>
        frameworkElement.SetValue( ObservedHeightProperty, observedHeight );

    private static void OnObserveChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
    {
        var frameworkElement = (FrameworkElement) dependencyObject;

        if( (bool) e.NewValue )
        {
            frameworkElement.SizeChanged += OnFrameworkElementSizeChanged;
            UpdateObservedSizesForFrameworkElement( frameworkElement );
        }
        else
        {
            frameworkElement.SizeChanged -= OnFrameworkElementSizeChanged;
        }
    }

    private static void OnFrameworkElementSizeChanged( object sender, SizeChangedEventArgs e ) =>
        UpdateObservedSizesForFrameworkElement( (FrameworkElement) sender );

    private static void UpdateObservedSizesForFrameworkElement( FrameworkElement frameworkElement )
    {
        // WPF 4.0 onwards
        frameworkElement.SetCurrentValue( ObservedWidthProperty, frameworkElement.ActualWidth );
        frameworkElement.SetCurrentValue( ObservedHeightProperty, frameworkElement.ActualHeight );

        // WPF 3.5 and prior
        ////SetObservedWidth(frameworkElement, frameworkElement.ActualWidth);
        ////SetObservedHeight(frameworkElement, frameworkElement.ActualHeight);
    }
}