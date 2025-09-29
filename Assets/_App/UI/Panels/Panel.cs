using System;
using UnityEngine;

namespace _App
{
    public class Panel<T> : BasePanel where T : IPanelSettings
    {
        public virtual void Setup(T messagePanelSettings)
        {
            
        }
    }
    
    public class BasePanel : MonoBehaviour{}
    
    public interface IPanelSettings
    {
        
    }
}