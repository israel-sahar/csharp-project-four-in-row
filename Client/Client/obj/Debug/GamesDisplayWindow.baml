   M S B A M L   `   `   `  ����A  =Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:ClientClient  5   	       Client.GamesDisplayWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   &localclr-namespace:Client     5   	   -   5          � Title$  Games��5   	   $��377.802��6   $��350.989��6(    � 
ResizeMode$ NoResize=�6O   ��5	      �� 5
   
    �� ImageSource$ images/login.png��6   .��5      �  � ColumnDefinitions 5   
   �� 5      �� 5       � RowDefinitions 5   
   �� 5      $
��0.3* �6   �� 5      $
��0.3* �6   �� 5      .+�5   
   �� -   5        $��lblName1��5   $   $y�Hidden=�6   $��Oblique$�64   $��Right=�6H   $��Center=�6d   $
��Bold"�6   6�   �� 5   
   -   5        $��lblName2��5   $   $y�Hidden=�6   $��1��64   $��Right=�6A   $��Center=�6]   $
��Bold"�6x   6�   �� 5   
   -   5        $��tbScore1��5   (   $y�Hidden=�6   $��1��68   $��Center=�6H   $	��5,0q�6c   Score6w   �� 5   
   -   5        $��tbScore2��5   (   $y�Hidden=�6   $��1��68   $��1��6E   $��Center=�6U   $	��5,0q�6p   Score6�   &   System.Windows.Controls.DataGrid  5   
   -   5        $��	GamesGrid��5   \   $��2��6   $��2��6    $��10,20,20,10q�64   
�������   CanUserAddRows .  $��Center=�6�   $y�Hidden=�6�   �� 5   
   -   5        $��NoGamesLabel��5   �   $��2��6   $��2��6    $��No games to show.��64   $��24(�6P   $��Heavy"�6^   
�������$��Center=�6�   $��Center=�6�   $y�Hidden=�6�   6�   5      5      