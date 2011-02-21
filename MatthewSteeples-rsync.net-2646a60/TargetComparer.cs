using System;
using System.Collections;
using System.Collections.Generic;

namespace NetSync
{
    public class TargetComparer : IComparer<Target>, IComparer
    {
        #region IComparer Members

        int IComparer.Compare(Object x, Object y)
        {
            return Match.CompareTargets((Target) x, (Target) y);
        }

        #endregion

        #region IComparer<Target> Members

        public int Compare(Target x, Target y)
        {
            return Match.CompareTargets(x, y);
        }

        #endregion
    }
}