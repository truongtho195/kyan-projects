   M S B A M L   `   `   `  ����a  ]PresentationFramework.Luna, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35gJclr-namespace:Microsoft.Windows.Themes;assembly=PresentationFramework.LunaMicrosoft.Windows.Themes  5       RichTextBoxControl+   %RichTextBoxControl.RichTextBoxToolbar   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      oMicrosoft_Windows_ThemesJclr-namespace:Microsoft.Windows.Themes;assembly=PresentationFramework.Luna     5      -   5          >�Name$  	ucControl��5       � 	Resources 5	         FlatButtonControlTemplate �� 
TargetType ��Name �� CornerRadius �� Triggers P� Property P� Value �� 
TargetName	 �� Property
 �� Value # FlatToggleButtonControlTemplate  
ListBorder �� Color �� 
TargetType-  'System.Windows.Input.KeyboardNavigation J� SnapsToDevicePixels ��Name �� Triggers  RichTextBoxStyle1 �� BasedOn  myComboBoxItemStyle �� RelativeSource �� AncestorType d�Name �� 
IsSelected d� 	IsChecked � ToolTip P� 
SourceName  ComboBoxColorStyle  ComboBoxFocusVisual �� SelectionBoxItem �� Padding � IsSharedSizeScope � ColumnDefinitions z� SharedSizeGroup " ComboBoxTransparentButtonStyle �� Mode �� Orientation �� AncestorLevel  �Name! � AllowsTransparency5   /Microsoft.Windows.Themes.SystemDropShadowChrome"  Name#   Color$ �� MaxDropDownHeight% � ActualWidth& �� ClearTypeHint' ��Name( �� ElementName) ��Name*   DirectionalNavigation+ ]� 
Conditions, �� Property- �� Value  ComboBoxEditableTemplate��%�   &	        &	 Q    &	 �    '�� �    7  '�� �    &	 �    7��&	 �    &	 �    7  7  �� 5   
   " ��5      .R�5      �� $ OuterBorder��6   
������ $��1q�5      $ 2��5      ._�5      �� $ InnerBorder��6   
������ 
������ $��1q�5      $ 2��5      ._�5      �� $��Center=�6'   $��Center=�5   '   ��. #��z��5   '   5      5       5      P� 5       �@J�IsMouseOver . . �5      �� $ OuterBorder��6   	 � ��
 �	#FF7CA0CC5!      �� 5"      $ InnerBorder��6   	 � ��
 �	#FFE4EFFD5$      �� 5%      $ InnerBorder��6   	 � ��
 �	#FFDAE7F55'      5(      P� 5)       � �� . . �5+      �� $ OuterBorder��6   	 � ��
 �	#FF2E4E765-      �� 5.      $ InnerBorder��6   	 � ��
 �	#FF116EE450      �� 51      $ InnerBorder��6   	 � ��
 �	#FF3272B853      54      56      �� 59   
   " d�5:      .R�5;      �� $ OuterBorder��6   
������ $��1q�5=      $ 2��5>      ._�5?      �� $ InnerBorder��6   
������ 
������ $��1q�5B      $ 2��5C      ._�5D      �� $��Center=�6'   $��Center=�5E   '   ��. #��z��5G   '   5H      5I       5J      P� 5K       �@J�IsMouseOver . . �5M      �� $ OuterBorder��6   	 � ��
 �	#FF7CA0CC5O      �� 5P      $ InnerBorder��6   	 � ��
 �	#FFE4EFFD5R      �� 5S      $ InnerBorder��6   	 � ��
 �	#FFDAE7F55U      5V      P� 5W       � �� . . �5Y      �� $ OuterBorder��6   	 � ��
 �	#FF2E4E765[      �� 5\      $ InnerBorder��6   	 � ��
 �	#FF116EE45^      �� 5_      $ InnerBorder��6   	 � ��
 �	#FF3272B85a      5b      P� 5c       �@d�	IsChecked$
 TrueY�5d      . �5e      �� $ OuterBorder��6   	 � ��
 �	#FFFFC00A5g      �� 5h      $ InnerBorder��6   	 � ��
 �	#FFFFDE7F5j      �� 5k      $ InnerBorder��6   	 � ��
 �	#FFFFD4585m      5n      5p      �� 5q   
   $ 	#FF7F9DB9��5r      �� 5s   
   " ��5t      .�5u      �� 	 � ��#
 � ��5v      �� 5w      	 � ��8
   5x      �� 5y      	 � ��#
 � ��5z      �� 5{      	 �@ TabNavigation$

 None=�5|      �� 5}      	 � ��$
 1q�5~      �� 5      	 � ��$

 Left=�5�      �� 5�      	 � ��$
 2q�5�      �� 5�      	 �@�FocusVisualStyle
 5�      Z� �� 5�      	 �@J�	AllowDrop
 . 5�      �� 5�      	 �@��PanningMode$
 VerticalFirst=�5�      �� 5�      	 �@��IsFlicksEnabled
 .  5�      �� 5�      	 � ��
 5�      �� 5�      " ��6&   .R�5�      �� $ Bd��6!   #��z��5�   !   #��z��5�   !   #��z��5�   !    . $��Center=�5�   !   $��Center=�5�   !   ._�5�      �� $ PART_ContentHost��6+   # z 5�   +   $��Center=�5�   +   $��Center=�5�   +   5�       5�      P� 5�       � {� .  . �5�   "   �� $ Bd��5�   )   #
 � ��5�   )   	 � ���� 5�   "   #
 � ��5�   )   	 � ��5�      5�      5�      5�      �� 5�   
   " ��5�      .�5�      �� 	 � ��

 ��  �5�      �� 5�      	 �@��TextDecorations$
 	Underline{�5�       5�      P� 5�       �@��IsMouseOver . . �5�      �� 	 � ��

 �  ��5�      5�      P� 5�       �@��	IsEnabled .  . �5�      �� 	 � ��#
 � ��5�      5�      P� 5�       �@��	IsEnabled . . �5�      �� 	 �@�Cursor$

 Hand��5�      5�      5�      �� 5�   
   " ��5�      8   .�5�      �� 	 � ��$
 10��5�      5�      �� 5�   
   " ��5�      .�5�      �� 	 � ��
 5�      �� *HorizontalContentAlignment+ �� " ���� 5�      	 � ��
 5�      �� *VerticalContentAlignment+ �� " ���� 5�      	 � ��$
 0q�5�      �� 5�      	 � ��
 �	#02FFFFFF5�      �� 5�      	 � ��
 5�      �� 5�      " ��6&   .R�5�      �� $ Bd��6!    . #��z��5�   !   #��z��5�   !   ���	#02B64141._�5�      d� $ 
tgbtnColor��6+   -   5        # z 5�   +    5�   +   �� *Name+��5�   +   �� *Name+��5�   +   �� *Name+5�   +   5�       5�      P� 5�      $ 
tgbtnColor��6&    �@d�	IsChecked$
 TrueY�5�   &   . �5�   "   �� 	 �@��
IsSelected
 . 5�   )   5�      P� 5�      $ 
tgbtnColor��6&    �@d�	IsChecked$ FalseY�5�   &   . �5�   "   �� 	 �@��
IsSelected
 .  5�   )   5�      5�      5�      �� 5�      	 � ��$
 20��5�      �� 5�      	 � ��$
 20��5�      �� 5�      	 � ��$
  ��5�      5�      �� 5�   
   " ��5�      .�5�      �� 	 �@�FocusVisualStyle#
 �  5      �� 5     	 � ��#
 � ��5     �� 5     	 � ��#
 � ��5     �� 5     	 � ��8
   5     �� 5     	 � ��$
 1q�5     �� 5	     	 � ��$
 1q�5
     �� 5     	 � ��$

 Auto=�5     �� 5     	 � ��$

 Auto=�5     �� 5     	 � ��
 . 5     �� 5     	 �@��PanningMode$

 Both=�5     �� 5     	 �@��IsFlicksEnabled
 .  5     �� 5     	 � ��$
 Center=�5     �� 5     	 � ��
 5     �� 5     " ��6&   .R�5     �  . ���	#00000000.+�5     �� $ Bd��6&   #��z��5     #��z��5     #��z 5     $ 1q�5     ._�5      �  .  5!     �� 5"     $��0 �6/   �� 5#     $��* �6/   �� 5$     $ ComboBoxButton��6/   $
��Auto �5%     .+�5'     �� $ SelectedItemBorder��6$   $��2��5(     #��z��5)     d� 54     $��3��6*   #���  56      55     �� *IsDropDownOpen+$ TwoWay=� �� *TemplatedParent+56     �� 57     $��1��6(   $ Vertical=�68   $��Center=�6j   .+�58     �� $��Center=�6(   $
��Wrap=�6E   $��A��6Y   $	��Top=�6b   $
��Bold"�6z   $��12(�6�   �� 59     $��4��6�   
���   �$
��0.15��6�   ��6(   �� *SelectedItem.Name+ �� *FindAncestor+$ 1��" ��6�   5:     5;     5<     � 5=     $  
PART_Popup��6!   ! . |�.  #��� .�5A     $��Bottom=�5B     ��5@     �� *IsDropDownOpen+ �� *TemplatedParent+.��5C       $
" Shdw��6L   $# Transparent��5D     #��z$ 5E     #��z% 5F     ._�5G     �� $ DropDownBorder��6$   #��� ��5H     $��1q�5I     #��� ��5J     ._�5K     �� $ DropDownScrollViewer��6+   .��5L     � $& Enabled=�6$   .+�5M      �� $
��Left=�6'   $��0��5N      $	��Top=�5O      $��0��5P      .+�5Q  !   �� $' 
OpaqueRect��6+   ��5R  !   �� *
Background+$( DropDownBorder����5S  !   �� *ActualHeight+$( DropDownBorder����5T  !   �� *ActualWidth+$( DropDownBorder��5U  !   �� 5V      $) ItemsPresenter��6/   $* 	Contained=�5W      # z 5X      5Y      5Z     5[     5\     5]     5^      5_     ]� 5`     + 5a  "   �� 5b  &   !, �@��IsSelectionBoxHighlighted- . 5c  0   �� 5d  &   , �@��IsDropDownOpen- .  5e  0   ./�5g  "   �� #
 � ��5h  )   	 � ��5i     P� 5j     ! �@��IsSelectionBoxHighlighted . . �5l  "   �� $ SelectedItemBorder��5m  )   #
 � ��5n  )   	 � ��5o     P� 5p     $ 
PART_Popup��5q  &    �@�HasDropShadow . . �5s  "   �� $
 Shdw��5t  )   	 � ��$
 0,0,5,5q�5u  )   �� 5v  "   $
 Shdw��5w  )   	 �@ Color$
 	#71000000��5x  )   5y     P� 5z      �@��HasItems .  . �5|  "   �� $ DropDownBorder��5}  )   	 � ��$
 95��5~  )   5     P� 5�      � {� .  . �5�  "   �� #
 � ��5�  )   	 � ���� 5�  "   $ Bd��5�  )   #
 � ��5�  )   	 � ��5�     P� 5�      �@��
IsGrouping . . �5�  "   �� 	 � ��
 .  5�  )   5�     P� 5�     $ DropDownScrollViewer��5�  &    � �� .  . �5�  "   �� $ 
OpaqueRect��5�  )   
 5�  )   �� *VerticalOffset+$( DropDownScrollViewer��	 �@��Top�� 5�  "   $ 
OpaqueRect��5�  )   
 5�  )   �� *HorizontalOffset+$( DropDownScrollViewer��	 �@��Left5�     5�     5�      5�     P� 5�      �@��
IsEditable . . �5�     �� 	 � ��
 .  5�     �� 5�     	 � ��$	
 0,1q�5�     �� 5�     	 � ��8
  5�     5�     5�     5�     .��5�     � ��6   �� *HorizontalAlignment+$( 	ucControl����5�     �� *VerticalAlignment+$( 	ucControl����5�     �� *
MinWidth+$( 	ucControl��$ TwoWay=�. �� UpdateSourceTrigger$. PropertyChanged=���5�     �� *	MinHeight+$( 	ucControl��$ TwoWay=�$. PropertyChanged=���5�     �� *	MaxHeight+$( 	ucControl��$ TwoWay=�$. PropertyChanged=���5�     �� *
MaxWidth+$( 	ucControl��$ TwoWay=�$. PropertyChanged=���5�     �� *Width+$( 	ucControl��$ TwoWay=�$. PropertyChanged=���5�     �� *Height+$( 	ucControl��$ TwoWay=�$. PropertyChanged=�.+�5�  
   �� -   5        / ��Name$/ 	rtContent��5�     $0 �� HorizontalScrollBarVisibility$
0 Auto=�5�     "1 �� VerticalScrollBarVisibility$
1 Auto=�5�     $��Stretch=�5�     $��Stretch=�5�     ��5�     �� *BorderBrush+$( 	ucControl��$ TwoWay=�$. PropertyChanged=���5�     �� *BorderThickness+$( 	ucControl��$ TwoWay=�$. PropertyChanged=���5�     �� *
Foreground+$( 	ucControl��$ TwoWay=�$. PropertyChanged=���5�     �� *
Background+$( 	ucControl��$ TwoWay=�$. PropertyChanged=�2 �� 
IsReadOnly2 5�     �� *
IsReadOnly+$( 	ucControl��$ TwoWay=�$. PropertyChanged=�5�     � 5�  
   -   5        $  popup��5�     $��Mouse=�5�     $��Slide=�5�     $��Center=�5�     .��5�     �� $��0q�6   $	��200��5�      . $��Center=�5�     $��Center=�5�     ��5�     �� *ToolbarBorderThickness+$( 	ucControl����5�     �� *ToolbarBorderBrush+$( 	ucControl����5�     �� *ToolbarBackground+$( 	ucControl��3 J� Effect3 5�     3  -System.Windows.Media.Effects.DropShadowEffect  5�     ._�5�     � 4 � Orientation$4 
Horizontal=�6   $��Center=�5�     $	��292��67   .+�5�     �� -   5        5 ��Name$5 FontFamilyCombo��5�     $��23��5�     $	��100��5�     $��5,2,0,2q�5�     6 �� 
IsEditable6 . ��5�     �� 5�     .R�5�  "   $� $	��250��69   5�     ��5�     �� 5�     .R�5�  "   �� $��15(�5�  ,   $��20��5�  ,   ��5�  ,   �� ��5�  ,   �� 5�  ,   5�     5�     �� 5�     -   5        $5 FontSizeCombo��5�     $��23��5�     $��40��5�     $��5,2,5,2q�5�     6 . 5�     �� 5�     -   5        $5 cbTextColor��5�     #���  5�     $$ 341.333333333333��5�     $��Center=�5�     #���  5�     $��23��5�     $��40��6+   ��5�     �� 5�     .R�5�     � $	��200��6)   5�     5�     �� 5�     $��ApplicationCommands.Cut��6   $	 Cut��5�     $��22��5�     ��5�     �� $( 	rtContent��.��5�     �� $��Images\editcut.png��6    $
��None=�5�       . 5�      5�     �� 5      $��ApplicationCommands.Copy��6   #��[  5     $
 Copy��5     $��22��5     ��5     �� $( 	rtContent��.��5     �� $��Images\editcopy.png��6    $
��None=�5       . 5      5     �� 5
     $��ApplicationCommands.Paste��6   #��[  5     $ Paste��5     $��22��5     ��5     �� $( 	rtContent��.��5     �� $��Images\editpaste.png��6    $
��None=�5       . 5      5     �� 5     $��ApplicationCommands.Undo��6   #��[  5     $
 Undo��5     $��22��5     ��5     �� $( 	rtContent��.��5     �� $��Images\editundo.png��6    $
��None=�5       . 5      5     �� 5     $��ApplicationCommands.Redo��6   #��[  5      $
 Redo��5!     $��22��5"     ��5     �� $( 	rtContent��.��5#     �� $��Images\editredo.png��6    $
��None=�5$       . 5%      5&     �� 5(     $��Images\separator.png��6   $
��None=�5)      . $��24��5+     d� 5-     -   5        $ 
BoldButton��5-  #   $ ��EditingCommands.ToggleBold��5.  #   #��[ 50  #   $
 Bold��51  #   $��22��52  #   ��5/  #   �� $( 	rtContent��.��53     �� $��Images\fontbold.png��6    $
��None=�54       . 55      56     d� 58     -	   5        $ ItalicButton��58  #   $"��EditingCommands.ToggleItalic��59  #   #��[ 5;  #   $ Italic��5<  #   $��22��5=  #   ��5:  #   �� $( 	rtContent��.��5>     �� $��Images\fontitalic.png��6    $
��None=�5?       . 5@      5A     d� 5C     -
   5        $ UnderlineButton��5C  #   $%��EditingCommands.ToggleUnderline��5D  #   #��[ 5F  #   $ 	Underline��5G  #   $��22��5H  #   ��5E  #   �� $( 	rtContent��.��5I     �� $��Images\fontunderline.png��6    $
��None=�5J       . 5K      5L     �� 5M     $��Images\separator.png��6   $
��None=�5N      . $��24��5P     �� 5R     -   5        7 ��Name$7 btnNormalText��5R     $&  Format as normal (non-code) text��5S     #��[  5U     $��22��5V     y�5T     �� *CodeControlsVisibility+ �� *FindAncestor+" >�.��5W     �� $��Images\normaltext.png��6    $
��None=�5X       . 5Y      5Z     �� 5p     $��Images\separator.png��6   $
��None=�5q      . $��24��5t     y�5s     �� *CodeControlsVisibility+ �� *FindAncestor+" >�5t     d� 5v     -   5        $ btnLeftButton��5v  #   $��EditingCommands.AlignLeft��5x  #   #��[ 5z  #   $ 
Align Left��5{  #   $��22��5|  #   ��5y  #   �� $( 	rtContent��.��5}     �� $��Images\alignleft.png��6    $
��None=�5~       . 5      5�     d� 5�     -   5        $ btnCenterButton��5�  #   $!��EditingCommands.AlignCenter��5�  #   #��[ 5�  #   $ Align Center��5�  #   $��22��5�  #   ��5�  #   �� $( 	rtContent��.��5�     �� $��Images\aligncenter.png��6    $
��None=�5�       . 5�      5�     d� 5�     -   5        $ btnRightButton��5�  #   $ ��EditingCommands.AlignRight��5�  #   #��[ 5�  #   $ Align Right��5�  #   $��22��5�  #   ��5�  #   �� $( 	rtContent��.��5�     �� $��Images\alignright.png��6    $
��None=�5�       . 5�      5�     d� 5�     -   5        $ JustifyButton��5�  #   $"��EditingCommands.AlignJustify��5�  #   #��[ 5�  #   $ Align Justify��5�  #   $��22��5�  #   ��5�  #   �� $( 	rtContent��.��5�     �� $��Images\alignjustify.png��6    $
��None=�5�       . 5�      5�     �� 5�     $��Images\separator.png��6   $
��None=�5�      . $��24��5�     d� 5�     -   5        $ btnBulletsButton��5�  #   $#��EditingCommands.ToggleBullets��5�  #   #��[ 5�  #   $ Bullets��5�  #   $��22��5�  #   ��5�  #   �� $( 	rtContent��.��5�     �� $��Images\listbullets.png��6    $
��None=�5�       . 5�      5�     d� 5�     -   5        $ btnNumberingButton��5�  #   $%��EditingCommands.ToggleNumbering��5�  #   #��[ 5�  #   $ 	Numbering��5�  #   $��22��5�  #   ��5�  #   �� $( 	rtContent��.��5�     �� $��Images\listnumbers.png��6    $
��None=�5�       . 5�      5�     �� 5�     $��Images\separator.png��6   $
��None=�5�     $��24��5�     �� 5�     $)��#EditingCommands.IncreaseIndentation��6   #��[  5�     $ Increase Indentation��5�     $��22��5�     ��5�     �� $( 	rtContent��.��5�     �� $��Images\indentincrease.png��6    $
��None=�5�       . 5�      5�     �� 5�     $)��#EditingCommands.DecreaseIndentation��6   #��[  5�     $ Decrease Indentation��5�     $��22��5�     ��5�     �� $( 	rtContent��.��5�     �� $��Images\indentdecrease.png��6    $
��None=�5�       . 5�      5�     5�     5�     5�     5�     5�     