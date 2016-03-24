﻿using System;
using System.Drawing;
using System.Windows.Forms;

using BSLib;
using GKCommon;
using GKCommon.GEDCOM;
using GKCore;
using GKCore.Interfaces;
using GKCore.Types;
using GKUI.Controls;
using GKUI.Sheets;

namespace GKUI.Dialogs
{
    /// <summary>
    /// 
    /// </summary>
    public partial class TfmPersonEdit : Form, IBaseEditor
	{
		private readonly IBaseWindow fBase;

        private readonly GKEventsSheet fEventsList;
        private readonly GKSheetList fSpousesList;
        private readonly GKSheetList fAssociationsList;
        private readonly GKSheetList fGroupsList;
		private readonly GKNotesSheet fNotesList;
        private readonly GKMediaSheet fMediaList;
		private readonly GKSourcesSheet fSourcesList;
        private readonly GKSheetList fUserRefList;
        private readonly GKSheetList fNamesList;

        private GEDCOMIndividualRecord fPerson;

        public GEDCOMIndividualRecord Person
		{
			get { return this.fPerson; }
			set { this.SetPerson(value); }
		}

		public IBaseWindow Base
		{
			get { return this.fBase; }
		}


		private void SetPerson(GEDCOMIndividualRecord value)
		{
			this.fPerson = value;
			try
			{
				string fam, nam, pat;
				this.fPerson.GetNameParts(out fam, out nam, out pat);
				this.edSurname.Text = fam;
				this.edName.Text = nam;
				this.edPatronymic.Text = pat;
				this.edSex.SelectedIndex = (sbyte)this.fPerson.Sex;
				this.chkPatriarch.Checked = this.fPerson.Patriarch;
				this.chkBookmark.Checked = this.fPerson.Bookmark;
				this.cbRestriction.SelectedIndex = (sbyte)this.fPerson.Restriction;

				this.UpdateControls();
			}
			catch (Exception ex)
			{
                this.fBase.Host.LogWrite("TfmPersonEdit.SetPerson(): " + ex.Message);
			}
		}

		private void UpdatePortrait()
		{
			Image img = this.fBase.Context.GetPrimaryBitmap(this.fPerson, this.imgPortrait.Width, this.imgPortrait.Height, false);

			if (img != null)
			{
				this.imgPortrait.Image = img; // освобождать нельзя, изображение исчезает
				this.imgPortrait.SizeMode = PictureBoxSizeMode.CenterImage;

				this.imgPortrait.Visible = true;
			}
			else
			{
				this.imgPortrait.Visible = false;
			}
		}

		private void UpdateControls()
		{
			if (this.fPerson.PersonalNames.Count > 0)
			{
				GEDCOMPersonalName np = this.fPerson.PersonalNames[0];
				this.edPiecePrefix.Text = np.Pieces.Prefix;
				this.edPieceNickname.Text = np.Pieces.Nickname;
				this.edPieceSurnamePrefix.Text = np.Pieces.SurnamePrefix;
				this.edPieceSuffix.Text = np.Pieces.Suffix;
			}

			if (this.fPerson.ChildToFamilyLinks.Count != 0)
			{
				GEDCOMFamilyRecord family = this.fPerson.ChildToFamilyLinks[0].Family;
				this.btnParentsAdd.Enabled = false;
				this.btnParentsEdit.Enabled = true;
				this.btnParentsDelete.Enabled = true;

				GEDCOMIndividualRecord relPerson = family.GetHusband();
				if (relPerson != null)
				{
					this.btnFatherAdd.Enabled = false;
					this.btnFatherDelete.Enabled = true;
					this.btnFatherSel.Enabled = true;
					this.EditFather.Text = relPerson.GetNameString(true, false);
				}
				else
				{
					this.btnFatherAdd.Enabled = true;
					this.btnFatherDelete.Enabled = false;
					this.btnFatherSel.Enabled = false;
					this.EditFather.Text = "";
				}

				relPerson = family.GetWife();
				if (relPerson != null)
				{
					this.btnMotherAdd.Enabled = false;
					this.btnMotherDelete.Enabled = true;
					this.btnMotherSel.Enabled = true;
					this.EditMother.Text = relPerson.GetNameString(true, false);
				}
				else
				{
					this.btnMotherAdd.Enabled = true;
					this.btnMotherDelete.Enabled = false;
					this.btnMotherSel.Enabled = false;
					this.EditMother.Text = "";
				}
			}
			else
			{
				this.btnParentsAdd.Enabled = true;
				this.btnParentsEdit.Enabled = false;
				this.btnParentsDelete.Enabled = false;
				this.btnFatherAdd.Enabled = true;
				this.btnFatherDelete.Enabled = false;
				this.btnFatherSel.Enabled = false;
				this.btnMotherAdd.Enabled = true;
				this.btnMotherDelete.Enabled = false;
				this.btnMotherSel.Enabled = false;
				this.EditFather.Text = "";
				this.EditMother.Text = "";
			}

		    this.fEventsList.DataList = this.fPerson.Events.GetEnumerator();
            this.fNotesList.DataList = this.fPerson.Notes.GetEnumerator();
		    this.fMediaList.DataList = this.fPerson.MultimediaLinks.GetEnumerator();
		    this.fSourcesList.DataList = this.fPerson.SourceCitations.GetEnumerator();
            this.UpdateSpousesSheet();
		    this.UpdateAssociationsSheet();
		    this.UpdateGroupsSheet();
		    this.UpdateURefsSheet();
		    this.UpdateNamesSheet();

			this.UpdatePortrait();

			this.LockEditor(this.fPerson.Restriction == GEDCOMRestriction.rnLocked);
		}

		private void LockEditor(bool locked)
		{
			this.edName.Enabled = !locked;
			this.edPatronymic.Enabled = !locked;
			this.edSurname.Enabled = !locked;
			
			this.edSex.Enabled = !locked;
			this.chkPatriarch.Enabled = !locked;
			this.chkBookmark.Enabled = !locked;

			this.edPiecePrefix.Enabled = !locked;
			this.edPieceNickname.Enabled = !locked;
			this.edPieceSurnamePrefix.Enabled = !locked;
			this.edPieceSuffix.Enabled = !locked;

			this.btnParentsAdd.Enabled = !locked;
			this.btnParentsEdit.Enabled = !locked;
			this.btnParentsDelete.Enabled = !locked;

			this.btnFatherAdd.Enabled = !locked;
			this.btnFatherDelete.Enabled = !locked;
			this.btnMotherAdd.Enabled = !locked;
			this.btnMotherDelete.Enabled = !locked;

			this.btnPortraitAdd.Enabled = !locked;
			this.btnPortraitDelete.Enabled = !locked;
			
			this.fEventsList.ReadOnly = locked;
			this.fNotesList.ReadOnly = locked;
			this.fMediaList.ReadOnly = locked;
			this.fSourcesList.ReadOnly = locked;
			this.fSpousesList.ReadOnly = locked;
			this.fAssociationsList.ReadOnly = locked;
			this.fGroupsList.ReadOnly = locked;
			this.fUserRefList.ReadOnly = locked;
		}

		private void cbRestriction_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.LockEditor(this.cbRestriction.SelectedIndex == (int)GEDCOMRestriction.rnLocked);
		}

		private void AcceptChanges()
		{
			GEDCOMPersonalName np = this.fPerson.PersonalNames[0];
			np.SetNameParts(this.edName.Text.Trim() + " " + this.edPatronymic.Text.Trim(), this.edSurname.Text.Trim(), np.LastPart);

			GEDCOMPersonalNamePieces pieces = np.Pieces;
			pieces.Nickname = this.edPieceNickname.Text;
			pieces.Prefix = this.edPiecePrefix.Text;
			pieces.SurnamePrefix = this.edPieceSurnamePrefix.Text;
			pieces.Suffix = this.edPieceSuffix.Text;

			this.fPerson.Sex = (GEDCOMSex)this.edSex.SelectedIndex;
			this.fPerson.Patriarch = this.chkPatriarch.Checked;
			this.fPerson.Bookmark = this.chkBookmark.Checked;
			this.fPerson.Restriction = (GEDCOMRestriction)this.cbRestriction.SelectedIndex;

			if (this.fPerson.ChildToFamilyLinks.Count > 0)
			{
				this.fPerson.ChildToFamilyLinks[0].Family.SortChilds();
			}

			this.fBase.ChangeRecord(this.fPerson);
		}

		private void btnAccept_Click(object sender, EventArgs e)
		{
			try
			{
				this.AcceptChanges();
				base.DialogResult = DialogResult.OK;
			}
			catch (Exception ex)
			{
                this.fBase.Host.LogWrite("TfmPersonEdit.btnAccept_Click(): " + ex.Message);
				base.DialogResult = DialogResult.None;
			}
		}

		private GKSheetList CreateAssociationsSheet(Control owner)
		{
			GKSheetList sheet = new GKSheetList(owner);
			
            sheet.Columns_BeginUpdate();
            sheet.AddColumn(LangMan.LS(LSID.LSID_Relation), 300, false);
            sheet.AddColumn(LangMan.LS(LSID.LSID_Person), 200, false);
            sheet.Columns_EndUpdate();

            sheet.Buttons = EnumSet<SheetButton>.Create(SheetButton.lbAdd, SheetButton.lbEdit, SheetButton.lbDelete, SheetButton.lbJump);
            sheet.OnModify += this.ModifyAssociationsSheet;
			
			return sheet;
		}
		
		private void UpdateAssociationsSheet()
		{
            try
            {
                fAssociationsList.ClearItems();

                foreach (GEDCOMAssociation ast in this.fPerson.Associations) {
                    string nm = ((ast.Individual == null) ? "" : ast.Individual.GetNameString(true, false));

                    GKListItem item = fAssociationsList.AddItem(ast.Relation, ast);
                    item.AddSubItem(nm);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWrite("TfmPersonEdit.UpdateAssociationsSheet(): " + ex.Message);
            }
		}
		
		private void ModifyAssociationsSheet(object sender, ModifyEventArgs eArgs)
		{
			bool result = false;

			GEDCOMAssociation ast = eArgs.ItemData as GEDCOMAssociation;

			switch (eArgs.Action)
			{
				case RecordAction.raAdd:
				case RecordAction.raEdit:
					TfmAssociationEdit fmAstEdit = new TfmAssociationEdit(this.fBase);
					try
					{
						if (eArgs.Action == RecordAction.raAdd) {
							ast = new GEDCOMAssociation(this.fBase.Tree, this.fPerson, "", "");
						}

						fmAstEdit.Association = ast;
						DialogResult res = TfmGEDKeeper.Instance.ShowModalEx(fmAstEdit, false);
						
						if (eArgs.Action == RecordAction.raAdd) {
							if (res == DialogResult.OK) {
								this.fPerson.Associations.Add(ast);
							} else {
								ast.Dispose();
							}
						}

						result = (res == DialogResult.OK);
					}
					finally
					{
						fmAstEdit.Dispose();
					}
					break;

				case RecordAction.raDelete:
					if (GKUtils.ShowQuestion(LangMan.LS(LSID.LSID_RemoveAssociationQuery)) != DialogResult.No)
					{
						this.fPerson.Associations.Delete(ast);
						result = true;
						this.fBase.Modified = true;
					}
					break;
					
				case RecordAction.raJump:
					if (ast != null) {
						this.AcceptChanges();
						this.fBase.SelectRecordByXRef(ast.Individual.XRef);
						base.Close();
					}
					break;
			}

			if (result) this.UpdateAssociationsSheet();
		}

		private GKSheetList CreateURefsSheet(Control owner)
		{
			GKSheetList sheet = new GKSheetList(owner);
			
            sheet.Columns_BeginUpdate();
            sheet.AddColumn(LangMan.LS(LSID.LSID_Reference), 300, false);
            sheet.AddColumn(LangMan.LS(LSID.LSID_Type), 200, false);
            sheet.Columns_EndUpdate();

            sheet.OnModify += this.ModifyURefsSheet;
			
			return sheet;
		}
		
		private void UpdateURefsSheet()
		{
            try
            {
                this.fUserRefList.ClearItems();

                foreach (GEDCOMUserReference uref in this.fPerson.UserReferences) {
                    GKListItem item = this.fUserRefList.AddItem(uref.StringValue, uref);
                    item.AddSubItem(uref.ReferenceType);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWrite("TfmPersonEdit.UpdateURefsSheet(): " + ex.Message);
            }
		}
		
		private void ModifyURefsSheet(object sender, ModifyEventArgs eArgs)
		{
            bool result = false;

            GEDCOMUserReference userRef = eArgs.ItemData as GEDCOMUserReference;

            switch (eArgs.Action) {
            	case RecordAction.raAdd:
            	case RecordAction.raEdit:
            		TfmUserRefEdit dlg = new TfmUserRefEdit(this.fBase);
            		try
            		{
            			if (eArgs.Action == RecordAction.raAdd) {
            				userRef = new GEDCOMUserReference(this.fBase.Tree, this.fPerson, "", "");
            			}

            			dlg.UserRef = userRef;
            			DialogResult res = TfmGEDKeeper.Instance.ShowModalEx(dlg, false);

            			if (eArgs.Action == RecordAction.raAdd) {
            				if (res == DialogResult.OK) {
            					this.fPerson.UserReferences.Add(userRef);
            				} else {
            					userRef.Dispose();
            				}
            			}

            			result = (res == DialogResult.OK);
            		}
            		finally
            		{
            			dlg.Dispose();
            		}
            		break;
            		
            	case RecordAction.raDelete:
            		if (GKUtils.ShowQuestion(LangMan.LS(LSID.LSID_RemoveUserRefQuery)) != DialogResult.No)
            		{
            			this.fPerson.UserReferences.Delete(userRef);
            			result = true;
            			this.fBase.Modified = true;
            		}
            		break;
            }
            
            if (result) this.UpdateURefsSheet();
		}

		private GKSheetList CreateSpousesSheet(Control owner)
		{
			GKSheetList sheet = new GKSheetList(owner);

            sheet.Columns_BeginUpdate();
            sheet.AddColumn("№", 25, false);
            sheet.AddColumn(LangMan.LS(LSID.LSID_Spouse), 300, false);
            sheet.AddColumn(LangMan.LS(LSID.LSID_MarriageDate), 100, false);
            sheet.Columns_EndUpdate();

            sheet.Buttons = EnumSet<SheetButton>.Create(SheetButton.lbAdd, SheetButton.lbEdit, SheetButton.lbDelete, 
				SheetButton.lbJump, SheetButton.lbMoveUp, SheetButton.lbMoveDown);
            sheet.OnModify += this.ModifySpousesSheet;
			
			return sheet;
		}

		private void UpdateSpousesSheet()
		{
            try
            {
                this.fSpousesList.ClearItems();

                int idx = 0;
                foreach (GEDCOMSpouseToFamilyLink spLink in this.fPerson.SpouseToFamilyLinks) {
                    idx += 1;

                    GEDCOMFamilyRecord family = spLink.Family;
                    if (family != null)
                    {
                        GEDCOMIndividualRecord relPerson;
                        string relName;

                        if (this.fPerson.Sex == GEDCOMSex.svMale) {
                        	relPerson = family.GetWife();
                            relName = LangMan.LS(LSID.LSID_UnkFemale);
                        } else {
                        	relPerson = family.GetHusband();
                            relName = LangMan.LS(LSID.LSID_UnkMale);
                        }

                        if (relPerson != null) {
                            relName = relPerson.GetNameString(true, false);
                        }

                        GKListItem item = this.fSpousesList.AddItem(idx, family);
                        item.AddSubItem(relName);
                        item.AddSubItem(GKUtils.GetMarriageDate(family));
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogWrite("TfmPersonEdit.UpdateSpousesSheet(): " + ex.Message);
            }
		}
		
		private void ModifySpousesSheet(object sender, ModifyEventArgs eArgs)
		{
            bool result = false;

            GEDCOMFamilyRecord family = eArgs.ItemData as GEDCOMFamilyRecord;

            switch (eArgs.Action)
            {
            	case RecordAction.raAdd:
            		result = (this.fBase.ModifyFamily(ref family, FamilyTarget.Spouse, this.fPerson));
            		if (result) eArgs.ItemData = family;
            		break;

            	case RecordAction.raEdit:
            		result = (this.fBase.ModifyFamily(ref family, FamilyTarget.None, null));
            		break;

            	case RecordAction.raDelete:
            		if (family != null && GKUtils.ShowQuestion(LangMan.LS(LSID.LSID_DetachSpouseQuery)) != DialogResult.No)
            		{
            			family.RemoveSpouse(this.fPerson);
            			result = true;
            		}
            		break;

            	case RecordAction.raMoveUp:
            	case RecordAction.raMoveDown:
            		{
            			int idx = this.fPerson.IndexOfSpouse(family);

            			switch (eArgs.Action)
            			{
            				case RecordAction.raMoveUp:
            					this.fPerson.ExchangeSpouses(idx - 1, idx);
            					break;

            				case RecordAction.raMoveDown:
            					this.fPerson.ExchangeSpouses(idx, idx + 1);
            					break;
            			}

            			result = true;
            			break;
            		}

            	case RecordAction.raJump:
            		if (family != null && (this.fPerson.Sex == GEDCOMSex.svMale || this.fPerson.Sex == GEDCOMSex.svFemale))
            		{
            			GEDCOMPointer sp = null;
            			switch (this.fPerson.Sex) {
            				case GEDCOMSex.svMale:
            					sp = family.Wife;
            					break;

            				case GEDCOMSex.svFemale:
            					sp = family.Husband;
            					break;
            			}

                        if (sp != null)
                        {
                            GEDCOMIndividualRecord spouse = (GEDCOMIndividualRecord)sp.Value;
                            this.AcceptChanges();
                            this.fBase.SelectRecordByXRef(spouse.XRef);
                            base.Close();
                        }
            		}
            		break;
            }

            if (result) this.UpdateSpousesSheet();
		}
		
		private GKSheetList CreateGroupsSheet(Control owner)
		{
			GKSheetList sheet = new GKSheetList(owner);
			
            sheet.Columns_BeginUpdate();
            sheet.AddColumn(LangMan.LS(LSID.LSID_Group), 350, false);
            sheet.Columns_EndUpdate();

            sheet.Buttons = EnumSet<SheetButton>.Create(SheetButton.lbAdd, SheetButton.lbDelete, SheetButton.lbJump);
            sheet.OnModify += this.ModifyGroupsSheet;
			
			return sheet;
		}

		private void UpdateGroupsSheet()
		{
            try
            {
                this.fGroupsList.ClearItems();

                foreach (GEDCOMPointer ptr in this.fPerson.Groups) {
                    GEDCOMGroupRecord grp = ptr.Value as GEDCOMGroupRecord;

                    if (grp != null) {
                        this.fGroupsList.AddItem(grp.GroupName, grp);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWrite("TfmPersonEdit.UpdateGroupsSheet(): " + ex.Message);
            }
		}
		
		private void ModifyGroupsSheet(object sender, ModifyEventArgs eArgs)
		{
			bool result = false;

			GEDCOMGroupRecord groupRec = eArgs.ItemData as GEDCOMGroupRecord;

			switch (eArgs.Action)
			{
				case RecordAction.raAdd:
					groupRec = this.fBase.SelectRecord(GEDCOMRecordType.rtGroup, null) as GEDCOMGroupRecord;
					result = (groupRec != null && groupRec.AddMember(this.fPerson));
					break;

				case RecordAction.raDelete:
					result = (GKUtils.ShowQuestion(LangMan.LS(LSID.LSID_DetachGroupQuery)) != DialogResult.No && groupRec.RemoveMember(this.fPerson));
					break;
					
				case RecordAction.raJump:
					if (groupRec != null) {
						this.AcceptChanges();
						this.fBase.SelectRecordByXRef(groupRec.XRef);
						base.Close();
					}
					break;
			}

			if (result) this.UpdateGroupsSheet();
		}


		private GKSheetList CreateNamesSheet(Control owner)
		{
			GKSheetList sheet = new GKSheetList(owner);
			
            sheet.Columns_BeginUpdate();
            sheet.AddColumn(LangMan.LS(LSID.LSID_Name), 350, false);
            sheet.AddColumn(LangMan.LS(LSID.LSID_Type), 100, false);
            sheet.Columns_EndUpdate();

            sheet.Buttons = EnumSet<SheetButton>.Create(SheetButton.lbAdd, SheetButton.lbEdit, SheetButton.lbDelete, 
                                                        SheetButton.lbMoveDown, SheetButton.lbMoveUp);
            sheet.OnModify += this.ModifyNamesSheet;
			
			return sheet;
		}


		private void UpdateNamesSheet()
		{
            try
            {
                this.fNamesList.ClearItems();

                foreach (GEDCOMPersonalName pn in this.fPerson.PersonalNames)
                {
                	GKListItem item = this.fNamesList.AddItem(pn.FullName, pn);
                	item.AddSubItem(LangMan.LS(GKData.NameTypes[(int)pn.NameType]));
                }
            }
            catch (Exception ex)
            {
                Logger.LogWrite("TfmPersonEdit.UpdateNamesSheet(): " + ex.Message);
            }
		}


		private void ModifyNamesSheet(object sender, ModifyEventArgs eArgs)
		{
			bool result = false;

			GEDCOMPersonalName persName = eArgs.ItemData as GEDCOMPersonalName;

			switch (eArgs.Action)
			{
				case RecordAction.raAdd:
				case RecordAction.raEdit:
            		TfmPersonalNameEdit dlg = new TfmPersonalNameEdit(this.fBase);
            		try
            		{
            			if (eArgs.Action == RecordAction.raAdd) {
            				persName = new GEDCOMPersonalName(this.fBase.Tree, this.fPerson, "", "");
            			}

            			dlg.PersonalName = persName;
            			DialogResult res = TfmGEDKeeper.Instance.ShowModalEx(dlg, false);

            			if (eArgs.Action == RecordAction.raAdd) {
            				if (res == DialogResult.OK) {
            					this.fPerson.PersonalNames.Add(persName);
            				} else {
            					persName.Dispose();
            				}
            			}

            			result = (res == DialogResult.OK);
            		}
            		finally
            		{
            			dlg.Dispose();
            		}
            		break;

				case RecordAction.raDelete:
            		if (this.fPerson.PersonalNames.Count > 1) {
						result = (GKUtils.ShowQuestion(LangMan.LS(LSID.LSID_RemoveNameQuery)) != DialogResult.No);
						if (result) {
							this.fPerson.PersonalNames.Delete(persName);
						}
            		} else {
            			GKUtils.ShowError(LangMan.LS(LSID.LSID_RemoveNameFailed));
            		}
					break;

				case RecordAction.raMoveUp:
				case RecordAction.raMoveDown:
					int idx = this.fPerson.PersonalNames.IndexOf(persName);
					switch (eArgs.Action)
					{
						case RecordAction.raMoveUp:
							this.fPerson.PersonalNames.Exchange(idx - 1, idx);
							break;

						case RecordAction.raMoveDown:
							this.fPerson.PersonalNames.Exchange(idx, idx + 1);
							break;
					}
					result = true;
					break;
			}

			if (result) this.UpdateNamesSheet();
		}
		

		private void SetTitle()
		{
            this.Text = string.Format("{0} \"{1} {2} {3}\" [{4}]", LangMan.LS(LSID.LSID_Person), this.edSurname.Text, this.edName.Text, 
                this.edPatronymic.Text, this.fPerson.GetXRefNum());
		}


		private void edSurname_TextChanged(object sender, EventArgs e)
		{
			this.SetTitle();
		}

		private void EditName_TextChanged(object sender, EventArgs e)
		{
			this.SetTitle();
		}

		private void EditPatronymic_TextChanged(object sender, EventArgs e)
		{
			this.SetTitle();
		}

		private void btnFatherAdd_Click(object sender, EventArgs e)
		{
			GEDCOMIndividualRecord father = this.fBase.SelectPerson(this.fPerson, TargetMode.tmChild, GEDCOMSex.svMale);
		    if (father == null) return;
		    
            GEDCOMFamilyRecord family = this.fBase.GetChildFamily(this.fPerson, true, father);
		    if (family.Husband.Value == null)
		    {
		        family.AddSpouse(father);
		    }
		    this.UpdateControls();
		}

		private void btnFatherDelete_Click(object sender, EventArgs e)
		{
			if (GKUtils.ShowQuestion(LangMan.LS(LSID.LSID_DetachFatherQuery)) != DialogResult.No)
			{
				GEDCOMFamilyRecord family = this.fBase.GetChildFamily(this.fPerson, false, null);
				if (family != null)
				{
					family.RemoveSpouse(family.GetHusband());
					this.UpdateControls();
				}
			}
		}

		private void btnFatherSel_Click(object sender, EventArgs e)
		{
			GEDCOMFamilyRecord family = this.fBase.GetChildFamily(this.fPerson, false, null);
		    if (family == null) return;
		    
            this.AcceptChanges();
            GEDCOMIndividualRecord father = family.GetHusband();
		    this.fBase.SelectRecordByXRef(father.XRef);
		    base.Close();
		}

		private void btnMotherAdd_Click(object sender, EventArgs e)
		{
			GEDCOMIndividualRecord mother = this.fBase.SelectPerson(this.fPerson, TargetMode.tmChild, GEDCOMSex.svFemale);
		    if (mother == null) return;
		    
            GEDCOMFamilyRecord family = this.fBase.GetChildFamily(this.fPerson, true, mother);
		    if (family.Wife.Value == null)
		    {
		        family.AddSpouse(mother);
		    }
		    this.UpdateControls();
		}

		private void btnMotherDelete_Click(object sender, EventArgs e)
		{
			if (GKUtils.ShowQuestion(LangMan.LS(LSID.LSID_DetachMotherQuery)) != DialogResult.No)
			{
				GEDCOMFamilyRecord family = this.fBase.GetChildFamily(this.fPerson, false, null);
				if (family != null)
				{
					GEDCOMIndividualRecord mother = family.GetWife();
					family.RemoveSpouse(mother);
					this.UpdateControls();
				}
			}
		}

		private void btnMotherSel_Click(object sender, EventArgs e)
		{
			GEDCOMFamilyRecord family = this.fBase.GetChildFamily(this.fPerson, false, null);
		    if (family == null) return;

            this.AcceptChanges();
            GEDCOMIndividualRecord mother = family.GetWife();
		    this.fBase.SelectRecordByXRef(mother.XRef);
		    base.Close();
		}

		private void btnParentsAdd_Click(object sender, EventArgs e)
		{
			GEDCOMFamilyRecord family = this.fBase.SelectFamily(this.fPerson);
		    if (family == null) return;

            if (family.IndexOfChild(this.fPerson) < 0)
		    {
		        family.AddChild(this.fPerson);
		    }
		    this.UpdateControls();
		}

		private void btnParentsEdit_Click(object sender, EventArgs e)
		{
			GEDCOMFamilyRecord family = this.fBase.GetChildFamily(this.fPerson, false, null);
			if (family != null && this.fBase.ModifyFamily(ref family, FamilyTarget.None, null))
			{
				this.UpdateControls();
			}
		}

		private void btnParentsDelete_Click(object sender, EventArgs e)
		{
			if (GKUtils.ShowQuestion(LangMan.LS(LSID.LSID_DetachParentsQuery)) != DialogResult.No)
			{
				GEDCOMFamilyRecord family = this.fBase.GetChildFamily(this.fPerson, false, null);
				if (family != null)
				{
					family.RemoveChild(this.fPerson);
					this.UpdateControls();
				}
			}
		}

		private void btnNameCopy1_Click(object sender, EventArgs e)
		{
			Clipboard.SetDataObject(this.fPerson.GetNameString(true, false));
		}

		private void btnPortraitAdd_Click(object sender, EventArgs e)
		{
			GEDCOMMultimediaRecord mmRec = fBase.SelectRecord(GEDCOMRecordType.rtMultimedia, null) as GEDCOMMultimediaRecord;
		    if (mmRec == null) return;
		    
            GEDCOMMultimediaLink mmLink = this.fPerson.GetPrimaryMultimediaLink();
		    if (mmLink != null)
		    {
		        mmLink.IsPrimary = false;
		    }
		    this.fPerson.SetPrimaryMultimediaLink(mmRec);
		    this.fMediaList.UpdateSheet();
		    this.UpdatePortrait();
		}

		private void btnPortraitDelete_Click(object sender, EventArgs e)
		{
			GEDCOMMultimediaLink mmLink = this.fPerson.GetPrimaryMultimediaLink();
		    if (mmLink == null) return;
		    
            mmLink.IsPrimary = false;
		    this.UpdatePortrait();
		}

		private void edSurname_KeyDown(object sender, KeyEventArgs e)
		{
            TextBox tb = (sender as TextBox);
            
            if (tb != null && e.KeyCode == Keys.Down && e.Control)
			{
				tb.Text = GEDCOMUtils.NormalizeName(tb.Text);
			}
		}

		private void edSurname_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '/')
			{
				e.Handled = true;
			}
		}

		public TfmPersonEdit(IBaseWindow aBase)
		{
			this.InitializeComponent();
			this.fBase = aBase;

			for (GEDCOMRestriction res = GEDCOMRestriction.rnNone; res <= GEDCOMRestriction.rnPrivacy; res++)
			{
				this.cbRestriction.Items.Add(LangMan.LS(GKData.Restrictions[(int)res]));
			}

			for (GEDCOMSex sx = GEDCOMSex.svNone; sx <= GEDCOMSex.svUndetermined; sx++)
			{
				this.edSex.Items.Add(GKUtils.SexStr(sx));
			}

            this.fEventsList = new GKEventsSheet(this, this.SheetEvents, true);

            this.fSpousesList = this.CreateSpousesSheet(this.SheetSpouses);
            this.fAssociationsList = this.CreateAssociationsSheet(this.SheetAssociations);
            this.fGroupsList = this.CreateGroupsSheet(this.SheetGroups);

			this.fNotesList = new GKNotesSheet(this, this.SheetNotes);
            this.fMediaList = new GKMediaSheet(this, this.SheetMultimedia);
			this.fSourcesList = new GKSourcesSheet(this, this.SheetSources);

			this.fUserRefList = this.CreateURefsSheet(this.SheetUserRefs);
			this.fNamesList = this.CreateNamesSheet(this.SheetNames);

			this.btnPortraitAdd.ImageList = TfmGEDKeeper.Instance.ImageList_Buttons;
			this.btnPortraitAdd.ImageIndex = 3;
			this.btnPortraitDelete.ImageList = TfmGEDKeeper.Instance.ImageList_Buttons;
			this.btnPortraitDelete.ImageIndex = 5;
			this.btnFatherAdd.ImageList = TfmGEDKeeper.Instance.ImageList_Buttons;
			this.btnFatherAdd.ImageIndex = 3;
			this.btnFatherDelete.ImageList = TfmGEDKeeper.Instance.ImageList_Buttons;
			this.btnFatherDelete.ImageIndex = 5;
			this.btnFatherSel.ImageList = TfmGEDKeeper.Instance.ImageList_Buttons;
			this.btnFatherSel.ImageIndex = 28;
			this.btnMotherAdd.ImageList = TfmGEDKeeper.Instance.ImageList_Buttons;
			this.btnMotherAdd.ImageIndex = 3;
			this.btnMotherDelete.ImageList = TfmGEDKeeper.Instance.ImageList_Buttons;
			this.btnMotherDelete.ImageIndex = 5;
			this.btnMotherSel.ImageList = TfmGEDKeeper.Instance.ImageList_Buttons;
			this.btnMotherSel.ImageIndex = 28;
			this.btnParentsAdd.ImageList = TfmGEDKeeper.Instance.ImageList_Buttons;
			this.btnParentsAdd.ImageIndex = 3;
			this.btnParentsEdit.ImageList = TfmGEDKeeper.Instance.ImageList_Buttons;
			this.btnParentsEdit.ImageIndex = 4;
			this.btnParentsDelete.ImageList = TfmGEDKeeper.Instance.ImageList_Buttons;
			this.btnParentsDelete.ImageIndex = 5;

			this.SetLang();
		}

		public void SetLang()
		{
			this.btnAccept.Text = LangMan.LS(LSID.LSID_DlgAccept);
			this.btnCancel.Text = LangMan.LS(LSID.LSID_DlgCancel);
			this.Text = LangMan.LS(LSID.LSID_WinPersonEdit);
			this.Label1.Text = LangMan.LS(LSID.LSID_Surname);
			this.Label2.Text = LangMan.LS(LSID.LSID_Name);
			this.Label3.Text = LangMan.LS(LSID.LSID_Patronymic);
			this.Label4.Text = LangMan.LS(LSID.LSID_Sex);
			this.Label7.Text = LangMan.LS(LSID.LSID_Nickname);
			this.Label8.Text = LangMan.LS(LSID.LSID_SurnamePrefix);
			this.Label6.Text = LangMan.LS(LSID.LSID_NamePrefix);
			this.Label9.Text = LangMan.LS(LSID.LSID_NameSuffix);
			this.chkPatriarch.Text = LangMan.LS(LSID.LSID_Patriarch);
			this.chkBookmark.Text = LangMan.LS(LSID.LSID_Bookmark);
			this.Label12.Text = LangMan.LS(LSID.LSID_Parents);
			this.SheetEvents.Text = LangMan.LS(LSID.LSID_Events);
			this.SheetSpouses.Text = LangMan.LS(LSID.LSID_Spouses);
			this.SheetAssociations.Text = LangMan.LS(LSID.LSID_Associations);
			this.SheetGroups.Text = LangMan.LS(LSID.LSID_RPGroups);
			this.SheetNotes.Text = LangMan.LS(LSID.LSID_RPNotes);
			this.SheetMultimedia.Text = LangMan.LS(LSID.LSID_RPMultimedia);
			this.SheetSources.Text = LangMan.LS(LSID.LSID_RPSources);
			this.SheetUserRefs.Text = LangMan.LS(LSID.LSID_UserRefs);
			this.Label5.Text = LangMan.LS(LSID.LSID_Restriction);
			this.SheetNames.Text = LangMan.LS(LSID.LSID_Names);
		}
	}
}
