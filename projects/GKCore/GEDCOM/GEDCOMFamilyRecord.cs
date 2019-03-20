﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2019 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;

using BSLib.Calendar;

namespace GKCommon.GEDCOM
{
    public sealed class GEDCOMFamilyRecord : GEDCOMRecordWithEvents
    {
        private static readonly GEDCOMFactory fTagsFactory;
        private GEDCOMList<GEDCOMPointer> fChildren;
        private GEDCOMList<GEDCOMSpouseSealing> fSpouseSealings;

        public GEDCOMList<GEDCOMPointer> Children
        {
            get { return fChildren; }
        }

        public GEDCOMPointer Husband
        {
            get { return TagClass(GEDCOMTagType.HUSB, GEDCOMPointer.Create) as GEDCOMPointer; }
        }

        public GEDCOMPointer Wife
        {
            get { return TagClass(GEDCOMTagType.WIFE, GEDCOMPointer.Create) as GEDCOMPointer; }
        }

        public GEDCOMPointer Submitter
        {
            get { return TagClass(GEDCOMTagType.SUBM, GEDCOMPointer.Create) as GEDCOMPointer; }
        }

        public GEDCOMRestriction Restriction
        {
            get { return GEDCOMUtils.GetRestrictionVal(GetTagStringValue(GEDCOMTagType.RESN)); }
            set { SetTagStringValue(GEDCOMTagType.RESN, GEDCOMUtils.GetRestrictionStr(value)); }
        }

        public GEDCOMList<GEDCOMSpouseSealing> SpouseSealings
        {
            get { return fSpouseSealings; }
        }

        public GKMarriageStatus Status
        {
            get { return GEDCOMUtils.GetMarriageStatusVal(GetTagStringValue("_STAT")); }
            set { SetTagStringValue("_STAT", GEDCOMUtils.GetMarriageStatusStr(value)); }
        }


        public GEDCOMFamilyRecord(GEDCOMTree owner, GEDCOMObject parent) : base(owner, parent)
        {
            SetRecordType(GEDCOMRecordType.rtFamily);
            SetName(GEDCOMTagType.FAM);

            fChildren = new GEDCOMList<GEDCOMPointer>(this);
            fSpouseSealings = new GEDCOMList<GEDCOMSpouseSealing>(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                fChildren.Dispose();
                fSpouseSealings.Dispose();
            }
            base.Dispose(disposing);
        }

        public GEDCOMIndividualRecord GetHusband()
        {
            return Husband.Value as GEDCOMIndividualRecord;
        }

        public GEDCOMIndividualRecord GetWife()
        {
            return Wife.Value as GEDCOMIndividualRecord;
        }

        static GEDCOMFamilyRecord()
        {
            GEDCOMFactory f = new GEDCOMFactory();
            fTagsFactory = f;

            f.RegisterTag(GEDCOMTagType.SLGS, GEDCOMSpouseSealing.Create);

            f.RegisterTag(GEDCOMTagType.ANUL, GEDCOMFamilyEvent.Create);
            f.RegisterTag(GEDCOMTagType.CENS, GEDCOMFamilyEvent.Create);
            f.RegisterTag(GEDCOMTagType.DIV, GEDCOMFamilyEvent.Create);
            f.RegisterTag(GEDCOMTagType.DIVF, GEDCOMFamilyEvent.Create);
            f.RegisterTag(GEDCOMTagType.ENGA, GEDCOMFamilyEvent.Create);
            f.RegisterTag(GEDCOMTagType.MARB, GEDCOMFamilyEvent.Create);
            f.RegisterTag(GEDCOMTagType.MARC, GEDCOMFamilyEvent.Create);
            f.RegisterTag(GEDCOMTagType.MARR, GEDCOMFamilyEvent.Create);
            f.RegisterTag(GEDCOMTagType.MARL, GEDCOMFamilyEvent.Create);
            f.RegisterTag(GEDCOMTagType.MARS, GEDCOMFamilyEvent.Create);
            f.RegisterTag(GEDCOMTagType.RESI, GEDCOMFamilyEvent.Create);
            f.RegisterTag(GEDCOMTagType.EVEN, GEDCOMFamilyEvent.Create);
        }

        public override GEDCOMTag AddTag(string tagName, string tagValue, TagConstructor tagConstructor)
        {
            GEDCOMTag result;

            if (tagName == GEDCOMTagType.HUSB || tagName == GEDCOMTagType.WIFE) {
                result = base.AddTag(tagName, tagValue, GEDCOMPointer.Create);
            } else if (tagName == GEDCOMTagType.CHIL) {
                result = fChildren.Add(new GEDCOMPointer(Owner, this, tagName, tagValue));
            } else {
                result = fTagsFactory.CreateTag(Owner, this, tagName, tagValue);

                if (result != null) {
                    if (result is GEDCOMFamilyEvent) {
                        result = AddEvent(result as GEDCOMFamilyEvent);
                    } else if (result is GEDCOMSpouseSealing) {
                        result = fSpouseSealings.Add(result as GEDCOMSpouseSealing);
                    }
                } else {
                    result = base.AddTag(tagName, tagValue, tagConstructor);
                }
            }

            return result;
        }

        public override GEDCOMCustomEvent AddEvent(GEDCOMCustomEvent evt)
        {
            if (evt != null) {
                if (evt is GEDCOMFamilyEvent) {
                    // SetLevel need for events created outside!
                    evt.SetLevel(Level + 1);
                    Events.Add(evt);
                } else {
                    throw new ArgumentException(@"Event has the invalid type", "evt");
                }
            }

            return evt;
        }

        public override void Clear()
        {
            base.Clear();

            // Special cleaning of Husband and Wife is not necessary,
            // because these are sub-tags and they will be cleared in the call base.Clear()

            int num = fChildren.Count;
            for (int i = 0; i < num; i++) {
                GEDCOMIndividualRecord child = (GEDCOMIndividualRecord)fChildren[i].Value;
                child.DeleteChildToFamilyLink(this);
            }
            fChildren.Clear();

            fSpouseSealings.Clear();
        }

        public override bool IsEmpty()
        {
            return base.IsEmpty() && fChildren.Count == 0 && fSpouseSealings.Count == 0;
        }

        public void DeleteChild(GEDCOMRecord childRec)
        {
            for (int i = fChildren.Count - 1; i >= 0; i--) {
                if (fChildren[i].Value == childRec) {
                    fChildren.DeleteAt(i);
                    break;
                }
            }
        }

        public int IndexOfChild(GEDCOMRecord childRec)
        {
            int result = -1;

            for (int i = fChildren.Count - 1; i >= 0; i--) {
                if (fChildren[i].Value == childRec) {
                    result = i;
                    break;
                }
            }

            return result;
        }

        public override void MoveTo(GEDCOMRecord targetRecord, bool clearDest)
        {
            GEDCOMFamilyRecord targetFamily = targetRecord as GEDCOMFamilyRecord;
            if (targetFamily == null)
                throw new ArgumentException(@"Argument is null or wrong type", "targetRecord");

            base.MoveTo(targetRecord, clearDest);

            while (fChildren.Count > 0) {
                GEDCOMPointer obj = fChildren.Extract(0);
                obj.ResetParent(targetFamily);
                targetFamily.Children.Add(obj);
            }

            while (fSpouseSealings.Count > 0) {
                GEDCOMSpouseSealing obj = fSpouseSealings.Extract(0);
                obj.ResetParent(targetFamily);
                targetFamily.SpouseSealings.Add(obj);
            }
        }

        public override void Pack()
        {
            base.Pack();

            fChildren.Pack();
            fSpouseSealings.Pack();
        }

        public override void ReplaceXRefs(XRefReplacer map)
        {
            base.ReplaceXRefs(map);

            if (Husband != null) {
                Husband.StringValue = GEDCOMUtils.EncloseXRef(map.FindNewXRef(Husband.StringValue));
            }

            if (Wife != null) {
                Wife.StringValue = GEDCOMUtils.EncloseXRef(map.FindNewXRef(Wife.StringValue));
            }

            fChildren.ReplaceXRefs(map);
            fSpouseSealings.ReplaceXRefs(map);
        }

        public override void ResetOwner(GEDCOMTree newOwner)
        {
            base.ResetOwner(newOwner);

            fChildren.ResetOwner(newOwner);
            fSpouseSealings.ResetOwner(newOwner);
        }

        public override void SaveToStream(StreamWriter stream)
        {
            base.SaveToStream(stream);

            fChildren.SaveToStream(stream);
            Events.SaveToStream(stream); // for files content compatibility
            fSpouseSealings.SaveToStream(stream);
        }

        private string GetFamilyString()
        {
            string result = "";

            GEDCOMIndividualRecord spouse = GetHusband();
            result += (spouse == null) ? "?" : spouse.GetPrimaryFullName();
            result += " - ";
            spouse = GetWife();
            result += (spouse == null) ? "?" : spouse.GetPrimaryFullName();

            return result;
        }

        public override float IsMatch(GEDCOMTag tag, MatchParams matchParams)
        {
            GEDCOMFamilyRecord otherFam = tag as GEDCOMFamilyRecord;
            if (otherFam == null) return 0.0f;

            float match = GetStrMatch(GetFamilyString(), otherFam.GetFamilyString(), matchParams);
            return match;
        }

        #region Auxiliary

        private static int EventsCompare(GEDCOMPointer cp1, GEDCOMPointer cp2)
        {
            UDN udn1 = ((GEDCOMIndividualRecord)cp1.Value).GetUDN(GEDCOMTagType.BIRT);
            UDN udn2 = ((GEDCOMIndividualRecord)cp2.Value).GetUDN(GEDCOMTagType.BIRT);
            return udn1.CompareTo(udn2);
        }

        public void SortChilds()
        {
            fChildren.Sort(EventsCompare);
        }

        public GEDCOMIndividualRecord GetSpouseBy(GEDCOMIndividualRecord spouse)
        {
            GEDCOMIndividualRecord husb = GetHusband();
            GEDCOMIndividualRecord wife = GetWife();

            return (spouse == husb) ? wife : husb;
        }

        public bool AddSpouse(GEDCOMIndividualRecord spouse)
        {
            if (spouse == null) {
                return false;
            }

            GEDCOMSex sex = spouse.Sex;
            if (sex == GEDCOMSex.svNone || sex == GEDCOMSex.svUndetermined) {
                return false;
            }

            switch (sex) {
                case GEDCOMSex.svMale:
                    Husband.Value = spouse;
                    break;

                case GEDCOMSex.svFemale:
                    Wife.Value = spouse;
                    break;
            }

            GEDCOMSpouseToFamilyLink spLink = new GEDCOMSpouseToFamilyLink(Owner, spouse);
            spLink.Family = this;
            spouse.SpouseToFamilyLinks.Add(spLink);

            return true;
        }

        public void RemoveSpouse(GEDCOMIndividualRecord spouse)
        {
            if (spouse == null) return;

            spouse.DeleteSpouseToFamilyLink(this);

            switch (spouse.Sex) {
                case GEDCOMSex.svMale:
                    Husband.Value = null;
                    break;

                case GEDCOMSex.svFemale:
                    Wife.Value = null;
                    break;
            }
        }

        public bool AddChild(GEDCOMIndividualRecord child)
        {
            if (child == null) return false;

            GEDCOMPointer ptr = new GEDCOMPointer(Owner, this);
            ptr.SetNameValue(GEDCOMTagType.CHIL, child);
            fChildren.Add(ptr);

            GEDCOMChildToFamilyLink chLink = new GEDCOMChildToFamilyLink(Owner, child);
            chLink.Family = this;
            child.ChildToFamilyLinks.Add(chLink);

            return true;
        }

        public bool RemoveChild(GEDCOMIndividualRecord child)
        {
            if (child == null) return false;

            DeleteChild(child);
            child.DeleteChildToFamilyLink(this);

            return true;
        }

        #endregion
    }
}
