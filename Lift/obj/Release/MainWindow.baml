   M S B A M L   `   `   `  ����   Lift    Lift.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     � Title$  Lift operation��$	��800��$	��600�� � WindowStartupLocation$ CenterScreen=� � WindowState$ Normal=����	#FFF0F0F0.��� .+��� -    ��Name$ floorListView��$	��233��$��17,41,0,9.6q�$
��Left=����� *FloorItemsList+���� .R��  � RowDefinitions �� $��80 � � ColumnDefinitions �� $��* ��� $��40 ��� $��* �.+��� $��0��$��Этаж��$��Center=�$��15(�$
��Bold"�$��10, 0, 0, 0q��� $��1��$��Center=�$��15(�$
��Bold"�$��10, 0, 0, 0q����� *floorNumber+�� $��2��$��
ВЫЗОВ��$��15(�$
��Bold"�$��90��$��60��
 � Tag �� *floorNumber+ �� UpdateSourceTrigger$ PropertyChanged=� �� Mode$ TwoWay=��� -   	 ��Name$	 floorsScrollViewer��$��0,41,24.6,9.6q�$��Stretch=�$��Right=�$	��200��.���� -   
 ��Name$
 showLift��
�������$	��Top=���  �� Orientation$ Vertical=�$��274,10,249.6,0q�
����ح�$	��176��$	��Top=�.+��� $��ПАРАМЕТРЫ:��$
��Bold"�$
��Left=��� $ 
Horizontal=�.+��� $(��"Количество этажей:��$
��Bold"�$
��Left=�$	��230���� $
��Left=�$
��Bold"� �� TextWrapping$ WrapWithOverflow=�$��28��$��23������  �� Path$ FloorsTotal�$ PropertyChanged=�$ TwoWay=��� $ 
Horizontal=�.+��� $"��Высота этажа, м:��$
��Bold"�$
��Left=�$	��Top=�$	��230���� $
��Left=�$
��Bold"�$
 Wrap=�$��28��$��23������ *FloorHeight+�� $ 
Horizontal=�.+��� $)��#Скорость лифта, м/с:��$
��Bold"�$
��Left=�$	��230���� $
��Left=�$
��Bold"�$
 Wrap=�$��20��$��28��$��23���� $ 
Horizontal=�.+��� $@��:Время открывания двери лифта, с:��$
��Bold"�$
��Left=�$	��230���� $
��Left=�$
��Bold"�$
 Wrap=�$��20��$��28��$��23���� $ 
Horizontal=�.+��� $@��:Время закрывания двери лифта, с:��$
��Bold"�$
��Left=�$	��230���� $
��Left=�$
��Bold"�$
 Wrap=�$��20��$��28��$��23���� -   $	 controlPanelScrollViewer��$
��Auto=�$��274,300,249.6,9.6q�.��� -    �Name$ controlWrapPanel�� � Orientation$ Vertical=�$	��500���� $2��,ПАНЕЛЬ УПРАВЛЕНИЯ ЛИФТА��$��274,269,249.6,0q�$
��Bold"�$	��Top=�$��Center=��� $9��3ПАНЕЛИ УПРАВЛЕНИЯ НА ЭТАЖАХ��$��	17,10,0,0q�$
��Bold"�$
��Left=�$	��Top=�$��Center=�$	��233���� $,��&СХЕМА ДВИЖЕНИЯ ЛИФТА��$��0,10,24.6,0q�$
��Bold"�$��Right=�$	��Top=�$��Center=�$	��200���� -   $	 testScrollViewer��$
��Auto=�$��274,191,249.6,0q�$��69��$	��Top=�.���� -    ��Name$ testTextBox��$
 Wrap=�