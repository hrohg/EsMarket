   M S B A M L   `   `   `  ����J  FEs.Market.Tools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null?$clr-namespace:Es.Market.Tools.ModelsEs.Market.Tools.Models  5      C&clr-namespace:Es.Market.Tools.ControlsEs.Market.Tools.Controls  5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089  �� DataType%    Es.Market.Tools.Models.PriceTag ��Name �� Path � RowDefinitions � ColumnDefinitions �� StringFormat � LayoutTransform �� Angle �� Orientation2   ,Es.Market.Tools.Controls.Ean13BarcodeControl	  Name
   	BarHeight   BarMinWidth   BarMaxWidth J� SnapsToDevicePixels   Barcode �� Triggers P� 
SourceName P� Property P� Value �� 
TargetName �� Value �� Property,   &Es.Market.Tools.Models.PriceDroppedTag �� Stretch	 �� X1 K� ZIndex �� MergedDictionaries�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      /models$clr-namespace:Es.Market.Tools.Models  5      3controls&clr-namespace:Es.Market.Tools.Controls  5       5p      %8  (��       5      *,  +)(�� �    58      *, +)�� 5      "    6   .R�5   
   � .+�5      �� $ Border��6   $��1q�6%   
���   �$��1q�6N   ��6Y   �� $ 
Size.Width���6{   �� $ Size.Height�._�5	      �  5
      �� 5      $��2* �6(   �� 5      $
��Auto �6(   �� 5      $��3* �6(    5      �� 5      $��* �6+   �� 5      $
��Auto �6+   .+�5      �� $��0��6    $��0��6-   $��2��6=   $��12(�6v   $��Black"�6�   $��WrapWithOverflow=�6�   ��6Q   �� *Product.Description+6�   �� 5      $��1��6    $��0��6-   $��2��6=   $	��3px��6Q   
���   �6^   �� 5      $��2��6    $��0��6-   $��45(�6y   $��	ExtraBold"�6�   $	��3 0q�6�   ��6=   �� *Product.Price+$ {0:#,##0.##}��6�   � 5      $��2��6   $��1��6(    5      �� 5      $��* �6,   �� 5      $��2* �6,    5      �� 5      $ 90Q�6.   .+�5      �� $��0��6%   $ 
Horizontal=�62   $��Center=�6K   .+�5       �� $��16(�6F   $	��3 0q�6T   $��Center=�6a   ��6(   �� *Product.Code+6a   5!        5#      $	 BarcodeControl��67   $��1��6O   $��20��5$   A   $
 30Q�5%   A   $ .5Q�5&   A   $ .5Q�5'   A   $��80��5(   A   $��70��5)   A   $	��Top=�5*   A   $��Center=�5+   7    .  5,   A   �� $ Product.Barcode�6j   5-      5.      5/      50       51   
   P� 52      $ Border��6    �@J�IsMouseOver . . �53      �� $ Border��6   # Z	�6C    � ��54      56      �� 58      "   6   .R�59   
   � .+�5:      �� $ Border��6   $��1q�6%   
���   �$��1q�6M   ��6X   �� $ 
Size.Width���6z   �� $ Size.Height�._�5;      �  5<      �� 5=      $
��Auto �6(   �� 5>      $��2* �6(   �� 5?      $
��Auto �6(   �� 5@      $��1* �6(   .+�5B      � $��0��6   $
��Left=�6(   .+�5C      �� $��16(�6d   $��	ExtraBold"�6r   $	��3 0q�6�   $��Oblique$�6�   
���  ��$
��Left=�6�   ��6%   �� *Product.OldPrice+$ {0:#,##0.##}��6�   �� 5D      $
 Fill=�6   
���   �$ 1��6=   $��0 4 0 -1q�6D    . $��34��6q   $	 -99��6}   5E      �� 5F      $��1��6    $��45(�6i   $��	ExtraBold"�6w   $	��3 0q�6�   
��� � �$��Center=�6�   $��Center=�6�   ��6-   �� *Product.Price+$ {0:#,##0.##}��6�   �� 5G      $��2��6    $��0��6-   $	��3px��6=   
���   �6J   � 5I      $��3��6    5J      �� 5K      $��* �6/   �� 5L      $
��Auto �6/   �� 5M      $
��Auto �6/   .+�5O      �� $��0��6$   $��12(�6Y   $��Black"�6g   $��WrapWithOverflow=�6z   $	��Top=�6�   ��64   �� *Product.Description+6�   � 5P      $��2��6   $	��3 0q�6/    5Q      �� 5R   "   $��* �60   �� 5S   "   $
��Auto �60   .+�5U      �� $��1��6(   $��10(�6S   $��Center=�6a   ��65   �� *Product.Code+6a     5V      $	 BarcodeControl��6;   $��0��6S   $��20��5W   ;   $
 25Q�5X   ;   $ 1Q�5Y   ;   $ 1Q�5Z   ;   $��70��5[   ;   $��Bottom=�5\   ;   $��Center=�5]   ;    .  5^   ;   �� $ Product.Barcode�6d   5c      5d      5e      5g      5h       5i   
   P� 5j      $ Border��6    �@J�IsMouseOver . . �5k      �� $ Border��6   # Z	�6C    � ��5l      5n      5s      