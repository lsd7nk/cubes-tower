using System;
using UnityEngine;

namespace _App
{
    public struct RefreshScrollerEvent : IEvent { }

    public struct OnButtonPressed : IEvent
    {
        public readonly Enum EnumType;
        
        public OnButtonPressed(Enum enumType)
        {
            EnumType = enumType;
        }
    }

    public struct OnButtonBackClicked : IEvent { }
    public struct OnButtonHomeClicked : IEvent { }
    
    public struct LanguageChangeEvent : IEvent
    {
        public readonly string LanguageName;
        public readonly Sprite LanguageIcon;

        public LanguageChangeEvent(string languageName, Sprite languageIcon)
        {
            LanguageName = languageName;
            LanguageIcon = languageIcon;
        }
    }
    
    public struct PopupCloseEvent : IEvent
    {
        public readonly string PopupId;

        public PopupCloseEvent(string popupId)
        {
            PopupId = popupId;
        }
    }
    
    public struct ShowPopupEvent : IEvent
    {
        public readonly Type Type;
        public readonly bool Animated;
        public readonly PopupSettings PopupSettings;

        public ShowPopupEvent(Type type, bool animated, PopupSettings popupSettings)
        {
            Type = type;
            Animated = animated;
            PopupSettings = popupSettings;
        }
    }
}