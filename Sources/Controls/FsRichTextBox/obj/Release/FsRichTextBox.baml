   M S B A M L   `   `   `  ����   FsRichTextBox/    )FsWpfControls.FsRichTextBox.FsRichTextBox   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     -     >�Name$  FsRichTextBoxControl�� � 	Resources    FlatButtonControlTemplate �� 
TargetType ��Name �� CornerRadius �� Triggers P� Property P� Value �� 
TargetName	 �� Property
 �� Value # FlatToggleButtonControlTemplate��%�  &	        &	     �� " ��.R��� $ OuterBorder��
������ $��1q�$ 2��._��� $ InnerBorder��
������ 
������ $��1q�$ 2��._��� $��Center=�$��Center=���. #��z�� P�  �@J�IsMouseOver . . ��� $ OuterBorder��	 � ��
 �	#FF7CA0CC�� $ InnerBorder��	 � ��
 �	#FFE4EFFD�� $ InnerBorder��	 � ��
 �	#FFDAE7F5P�  � �� . . ��� $ OuterBorder��	 � ��
 �	#FF2E4E76�� $ InnerBorder��	 � ��
 �	#FF116EE4�� $ InnerBorder��	 � ��
 �	#FF3272B8�� " d�.R��� $ OuterBorder��
������ $��1q�$ 2��._��� $ InnerBorder��
������ 
������ $��1q�$ 2��._��� $��Center=�$��Center=���. #��z�� P�  �@J�IsMouseOver . . ��� $ OuterBorder��	 � ��
 �	#FF7CA0CC�� $ InnerBorder��	 � ��
 �	#FFE4EFFD�� $ InnerBorder��	 � ��
 �	#FFDAE7F5P�  � �� . . ��� $ OuterBorder��	 � ��
 �	#FF2E4E76�� $ InnerBorder��	 � ��
 �	#FF116EE4�� $ InnerBorder��	 � ��
 �	#FF3272B8P�  �@d�	IsChecked$
 TrueY�. ��� $ OuterBorder��	 � ��
 �	#FFFFC00A�� $ InnerBorder��	 � ��
 �	#FFFFDE7F�� $ InnerBorder��	 � ��
 �	#FFFFD458.��]� ��. .+��� $	��Top=� J� SnapsToDevicePixels . ����  �� ElementName$ FsRichTextBoxControl�� �� Path$ ToolbarBorderThickness����� $ FsRichTextBoxControl��$ ToolbarBorderBrush�._���  �� Orientation$ 
Horizontal=�$��24������ $ FsRichTextBoxControl��$ ToolbarBackground�.+��� -    ��Name$ FontFamilyCombo��$��23��$	��100��$��5,2,0,2q� �� 
IsEditable . ���� .R�$� $	��250������ .R��� $��15(�$��20������ ���� �� -   $ FontSizeCombo��$��23��$��40��$��5,2,5,2q� . �� $��ApplicationCommands.Cut�� � ToolTip$	 Cut��#��[  $��0,1,0,1q����� $ TextBox��.���� $��Images\editcut.png��$
��None=� . �� $��ApplicationCommands.Copy��$
 Copy��#��[  $��0,1,0,1q����� $ TextBox��.���� $��Images\editcopy.png��$
��None=� . �� $��ApplicationCommands.Paste��$ Paste��#��[  $��0,1,0,1q����� $ TextBox��.���� $��Images\editpaste.png��$
��None=� . �� $��ApplicationCommands.Undo��$
 Undo��#��[  $��0,1,0,1q����� $ TextBox��.���� $��Images\editundo.png��$
��None=� . �� $��ApplicationCommands.Redo��$
 Redo��#��[  $��0,1,0,1q����� $ TextBox��.���� $��Images\editredo.png��$
��None=� . �� $��Images\separator.png��$
��None=� . d� -    d�Name$ 
BoldButton��$ ��EditingCommands.ToggleBold��$
 Bold��#��[ $��0,1,0,1q����� $ TextBox��.���� $��Images\fontbold.png��$
��None=� . d� -   $ ItalicButton��$"��EditingCommands.ToggleItalic��$ Italic��#��[ $��0,1,0,1q����� $ TextBox��.���� $��Images\fontitalic.png��$
��None=� . d� -   $ UnderlineButton��$%��EditingCommands.ToggleUnderline��$ 	Underline��#��[ $��0,1,0,1q����� $ TextBox��.���� $��Images\fontunderline.png��$
��None=� . �� $��Images\separator.png��$
��None=� . �� -   $&  Format as normal (non-code) text��#��[  $��0,1,0,1q�y��� $ CodeControlsVisibility� �� RelativeSource �� *FindAncestor+ �� AncestorType" >�.���� $��Images\normaltext.png��$
��None=� . �� -   $ Format as code block��#��[  $��0,1,0,1q�y��� $ CodeControlsVisibility� �� *FindAncestor+" >�.���� $��Images\codeblock.png��$
��None=� . �� -	   $ Format as inline code��#��[  $��0,1,0,1q�y��� $ CodeControlsVisibility� �� *FindAncestor+" >�.���� $��Images\codeinline.png��$
��None=� . �� $��Images\separator.png��$
��None=� . y��� $ CodeControlsVisibility� �� *FindAncestor+" >�d� -
   $ 
LeftButton��$��EditingCommands.AlignLeft��$ 
Align Left��#��[ $��0,1,0,1q����� $ TextBox��.���� $��Images\alignleft.png��$
��None=� . d� -   $ CenterButton��$!��EditingCommands.AlignCenter��$ Align Center��#��[ $��0,1,0,1q����� $ TextBox��.���� $��Images\aligncenter.png��$
��None=� . d� -   $ RightButton��$ ��EditingCommands.AlignRight��$ Align Right��#��[ $��0,1,0,1q����� $ TextBox��.���� $��Images\alignright.png��$
��None=� . d� -   $ JustifyButton��$"��EditingCommands.AlignJustify��$ Align Justify��#��[ $��0,1,0,1q����� $ TextBox��.���� $��Images\alignjustify.png��$
��None=� . �� $��Images\separator.png��$
��None=� . d� -   $ BulletsButton��$#��EditingCommands.ToggleBullets��$ Bullets��#��[ $��0,1,0,1q����� $ TextBox��.���� $��Images\listbullets.png��$
��None=� . d� -   $ NumberingButton��$%��EditingCommands.ToggleNumbering��$ 	Numbering��#��[ $��0,1,0,1q����� $ TextBox��.���� $��Images\listnumbers.png��$
��None=� . �� $��Images\separator.png��$
��None=��� $)��#EditingCommands.IncreaseIndentation��$ Increase Indentation��#��[  $��0,1,0,1q����� $ TextBox��.���� $��Images\indentincrease.png��$
��None=� . �� $)��#EditingCommands.DecreaseIndentation��$ Decrease Indentation��#��[  $��0,1,0,1q����� $ TextBox��.���� $��Images\indentdecrease.png��$
��None=� . �� -    ��Name$ TextBox��&   System.Windows.Media.TextOptions   TextFormattingMode$ Ideal=�   TextRenderingMode$ Aliased=� �� 
AcceptsTab .  �� AcceptsReturn . " �� VerticalScrollBarVisibility$
 Auto=�{��� $ 	IsEnabled� �� *FindAncestor+" >�