   M S B A M L   `   `   `  ����  Es.Market.Tools+    %Es.Market.Tools.Controls.LabelPrinter   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5        � Title  5   7   �� *Title+ � 	Resources 5	       �� MergedDictionaries �� Source   ListBoxItemStyle �� 
TargetType �� Property �� Value �� 
TargetType ��Name	 �� Padding
 J� SnapsToDevicePixels J� InputBindings d� MouseAction �� Command �� Path �� RelativeSource �� AncestorType �� CommandParameter � ContextMenu � DataContext m� Command m� CommandParameter �� Triggers P� Property P� Value �� 
TargetName�� 5   
    5      �� 5      $ ../Views/Generic.xaml?�6%   %?  &	        �� 5      " ��6-   .�5      ��  � �� 5      �� 5      " ��6*   .R�5      �� $ Bd��6%   #��z��61   #��z��6]   #��z��6�   #	 z��6�   
 . ._�5   "   �� #��z��63   #
 z
 6v   #��z��6�    5   "   d� 5   &   $ LeftDoubleClicke�5   3    5   3   �� $  DataContext.AddItemCommand� �� *FindAncestor+" �� 5   3   �� 5   3    5   "   �� 5   &    62   �� .@�5   *   m� $��Ավելացնել��63    6F   �� $  DataContext.AddItemCommand� �� *FindAncestor+" �� 5   3   �� 5   '   5!       5"      P� 5#   "    �@��
IsSelected . . �5$   &   �� # Z�6C    � ��5%   #   P� 5&   "    �@J�IsMouseOver . . �5'   &   �� $ Bd��6-   # Z	�6S    � ��5(   #   5*      5,      5-      5F      .��5H      �  � RowDefinitions 5I   
   �� 5J      $
��Auto �6   �� 5K      $��* �6   �� 5L      $
��Auto �6   .+�5N   
   � $��1��6    � ColumnDefinitions 5O      �� 5P      $
��Auto �6#   �� 5Q      $��3 �6#   �� 5R      $��* �6#   .+�5T      � $��0��6    5U      �� 5V      $��* �6$   �� 5W      $��3 �6$   �� 5X      $
��Auto �6$   .+�5Z      �� $��Ապրանքներ��6   $��0��6.   .��5[      �� .+�5\      �� $	��Top=�6c   $��1q�6{   ��6"   �� *
ProductKey+ �� UpdateSourceTrigger$ PropertyChanged=�6{   �� 5]      -   5         ��Name$ ProductItems��5]   "   #��[  6Y   ��68   �� *
Products+��5^      �� 5_   "   .R�5`   &   �� $��0q�6-   ._�5a   *   ��  �� Orientation$ 
Horizontal=�65   .+�5b   .   �� ��68   �� *Code+�� 5c   .   ��68   �� *Description+5d   +   5e   '   5f   #   5h      5i      5j      �� 5k      $��1��6   $��3��6,   $��Stretch=�67   $��Center=�6U   �� 5l      $��2��6   .��5m      ��  �� SelectionMode$ Single=�6t   ��6   �� *LabelTemplates+  �� SelectedItem  6E   �� *SelectedLabelTemplate+5n      5o      5p      �� 5r      $��1��6   $��Stretch=�6,   $��Center=�6H   $��3��6e   �� 5s      $��Գնապիտակներ��6   $��2��6,   .��5t      �� $
��Auto=�6   .��5u      � -   5        ! �Name$
! Grid��5u      .+�5v      �� ��6'   �� $ Labels���5w      �� 5x   "   .R�5y   &   � " � Orientation$" 
Horizontal=�60   $
��21cm��6I   5z   #   ��5|      �� 5}   "   " ��6(   .�5~   &   ��  �@�Tag 6<   �� # �� ElementName$
# Grid��$ DataContext��� 5   &    �@�ContextMenu 5�   *   �� 5�   .    6:   �� $ PlacementTarget�$ �� Self# Z$ .@�5�   2   m� $��Հեռացնել��6;    6M   �� $ Tag.RemoveItemCommand� 6~   �� $ DataContext�5�   /   5�   '   5�   #   5�      5�      5�      5�      5�      �� 5�   
   $��2��6   $ 
Horizontal=�6"   $��Right=�6;   $��5q�6W   .+�5�      �� $��Տպել��6   ��6$   �� *PrintCommand+��6E   �� $
# Grid���� 5�      $��Մաքրել��6   ��6&   �� *CleanCommand+�� 5�      $��
Փակել��6   ��6%   �� *CloseCommand+5�      5�      5�      