using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HMUI;
using TMPro;
using System.Linq;

namespace HMUI {

    public class ScrollToTopOnEnable : MonoBehaviour {

        [SerializeField] ScrollView _scrollView = default;
        
        protected void OnEnable() {
            
            _scrollView.ScrollTo(0.0f, animated: false);
        }
    }
}
