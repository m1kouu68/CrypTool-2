﻿/*
   Copyright 2023 Nils Kopal, CrypTool project

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
namespace CrypTool.Typex.TypexMachine
{
    public class Stator : Rotor
    {
        /// <summary>
        /// Creates a stator
        /// </summary>
        /// <param name="rotor"></param>
        /// <param name="notches"></param>
        /// <param name="rotation"></param>
        /// <param name="isReversed"></param>
        public Stator(int[] rotor, int[] notches, int rotation, bool isReversed) : base(rotor, notches, rotation, false, isReversed)
        {
        }

        public override void Step()
        {
        }

        public override bool IsAtNotchPosition()
        {
            return true;
        }

        public override bool WillBeAtNotchPosition()
        {
            return true;
        }
    }
}
