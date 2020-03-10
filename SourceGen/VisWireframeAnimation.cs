﻿/*
 * Copyright 2020 faddenSoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommonWPF;
using PluginCommon;

namespace SourceGen {
    /// <summary>
    /// A wireframe visualization with animation.
    /// </summary>
    /// <remarks>
    /// All of the animation parameters get added to the Visualization parameter set, so this
    /// is just a place to hold the IVisualizationWireframe reference and some constants.
    /// </remarks>
    public class VisWireframeAnimation : Visualization {
        /// <summary>
        /// Frame delay parameter.
        /// </summary>
        public const string P_FRAME_DELAY_MSEC = "_frameDelayMsec";

        /// <summary>
        /// Frame count parameter.
        /// </summary>
        public const string P_FRAME_COUNT = "_frameCount";

        public const string P_IS_ANIMATED = "_isAnimatedWireframe";

        public const string P_EULER_ROT_X = "_eulerRotX";
        public const string P_EULER_ROT_Y = "_eulerRotY";
        public const string P_EULER_ROT_Z = "_eulerRotZ";

        public const string P_DELTA_ROT_X = "_deltaRotX";
        public const string P_DELTA_ROT_Y = "_deltaRotY";
        public const string P_DELTA_ROT_Z = "_deltaRotZ";

        private WireframeObject mWireObj;


        /// <summary>
        /// Constructor.  Mostly pass-through, but we want to set the overlay image.
        /// </summary>
        public VisWireframeAnimation(string tag, string visGenIdent,
                ReadOnlyDictionary<string, object> visGenParams, Visualization oldObj,
                WireframeObject wireObj)
                : base(tag, visGenIdent, visGenParams, oldObj) {
            // wireObj may be null when loading from project file
            mWireObj = wireObj;
            OverlayImage = ANIM_OVERLAY_IMAGE;
        }

        /// <summary>
        /// Updates the thumbnail.
        /// </summary>
        /// <remarks>
        /// We override it because this is our first opportunity to capture the
        /// wireframe object reference if the object was created during project
        /// file loading.
        /// </remarks>
        /// <param name="visWire">Reference to wireframe data generated by plugin.</param>
        /// <param name="parms">Render parameters.</param>
        public override void SetThumbnail(IVisualizationWireframe visWire,
                ReadOnlyDictionary<string, object> parms) {
            base.SetThumbnail(visWire, parms);
            mWireObj = WireframeObject.Create(visWire);
        }

        /// <summary>
        /// Generates an animated GIF from a series of frames.
        /// </summary>
        /// <param name="encoder">GIF encoder.</param>
        /// <param name="dim">Output dimensions.</param>
        public void EncodeGif(AnimatedGifEncoder encoder, double dim) {
            int curX = Util.GetFromObjDict(VisGenParams, P_EULER_ROT_X, 0);
            int curY = Util.GetFromObjDict(VisGenParams, P_EULER_ROT_Y, 0);
            int curZ = Util.GetFromObjDict(VisGenParams, P_EULER_ROT_Z, 0);
            int deltaX = Util.GetFromObjDict(VisGenParams, P_DELTA_ROT_X, 0);
            int deltaY = Util.GetFromObjDict(VisGenParams, P_DELTA_ROT_Y, 0);
            int deltaZ = Util.GetFromObjDict(VisGenParams, P_DELTA_ROT_Z, 0);
            int frameCount = Util.GetFromObjDict(VisGenParams, P_FRAME_COUNT, 1);
            int frameDelayMsec = Util.GetFromObjDict(VisGenParams, P_FRAME_DELAY_MSEC, 100);
            bool doPersp = Util.GetFromObjDict(VisGenParams, VisWireframe.P_IS_PERSPECTIVE, true);
            bool doBfc = Util.GetFromObjDict(VisGenParams, VisWireframe.P_IS_BFC_ENABLED, false);

            for (int frame = 0; frame < frameCount; frame++) {
                BitmapSource bs = GenerateWireframeImage(mWireObj, dim,
                    curX, curY, curZ, doPersp, doBfc);
                encoder.AddFrame(BitmapFrame.Create(bs), frameDelayMsec);

                curX = (curX + 360 + deltaX) % 360;
                curY = (curY + 360 + deltaY) % 360;
                curZ = (curZ + 360 + deltaZ) % 360;
            }
        }
    }
}
