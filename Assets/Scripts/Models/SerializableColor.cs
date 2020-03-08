﻿using UnityEngine;

namespace Models
{
    [System.Serializable]
    public class SerializableColor{
     
        public    float[]            colorStore = new float[4]{1F,1F,1F,1F};
        public    Color            Color{
            get{ return new Color( colorStore[0], colorStore[1], colorStore[2], colorStore[3] );}
            set{ colorStore = new float[4]{ value.r, value.g, value.b, value.a  };                }
        }
 
        public static implicit operator Color ( SerializableColor instance ){
            return instance.Color;
        }
 
        public static implicit operator SerializableColor ( Color color ){
            return new SerializableColor{ Color = color};
        }
    }
}