﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2017 by Sergey V. Zhdanovskih.
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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GKCommon.GEDCOM
{
    /// <summary>
    /// 
    /// </summary>
    public class GEDCOMProvider
    {
        public const char GEDCOM_DELIMITER = ' ';
        public const char GEDCOM_YEAR_MODIFIER_SEPARATOR = '/';
        public const string GEDCOM_YEAR_BC = "B.C.";
        public const char GEDCOM_POINTER_DELIMITER = '@';
        public const string GEDCOM_NEWLINE = "\r\n";


        // deprecated
        //public const byte GEDCOMMaxPhoneNumbers = 3;
        //public const byte GEDCOMMaxEmailAddresses = 3;
        //public const byte GEDCOMMaxFaxNumbers = 3;
        //public const byte GEDCOMMaxWebPages = 3;
        //public const byte GEDCOMMaxLanguages = 3;


        private readonly GEDCOMTree fTree;


        public GEDCOMProvider(GEDCOMTree tree)
        {
            fTree = tree;
        }

        #region Encoding hack

        private enum EncodingState { esUnchecked, esUnchanged, esChanged }

        private const int DEF_CODEPAGE = 1251;
        private static readonly Encoding DEFAULT_ENCODING = Encoding.GetEncoding(DEF_CODEPAGE);

        private static string ConvertStr(Encoding encoding, string str)
        {
            byte[] src = DEFAULT_ENCODING.GetBytes(str);
            str = encoding.GetString(src);
            return str;
        }

        private void DefineEncoding(StreamReader reader, ref Encoding sourceEncoding, ref EncodingState encodingState)
        {
            GEDCOMCharacterSet charSet = fTree.Header.CharacterSet;
            switch (charSet)
            {
                case GEDCOMCharacterSet.csUTF8:
                    if (!SysUtils.IsUnicodeEncoding(reader.CurrentEncoding)) {
                        sourceEncoding = Encoding.UTF8;
                        encodingState = EncodingState.esChanged; // file without BOM
                    } else {
                        encodingState = EncodingState.esUnchanged;
                    }
                    break;

                case GEDCOMCharacterSet.csUNICODE:
                    if (!SysUtils.IsUnicodeEncoding(reader.CurrentEncoding)) {
                        sourceEncoding = Encoding.Unicode;
                        encodingState = EncodingState.esChanged; // file without BOM
                    } else {
                        encodingState = EncodingState.esUnchanged;
                    }
                    break;

                case GEDCOMCharacterSet.csASCII:
                    string cpVers = fTree.Header.CharacterSetVersion;
                    if (!string.IsNullOrEmpty(cpVers)) {
                        int sourceCodepage = SysUtils.ParseInt(cpVers, DEF_CODEPAGE);
                        sourceEncoding = Encoding.GetEncoding(sourceCodepage);
                        encodingState = EncodingState.esChanged;
                    } else {
                        sourceEncoding = Encoding.GetEncoding(DEF_CODEPAGE);
                        encodingState = EncodingState.esChanged;
                    }
                    break;
            }
        }

        #endregion

        #region Loading functions

        public void LoadFromFile(string fileName)
        {
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
                LoadFromStreamExt(fileStream, fileStream, fileName);
            }
        }

        public void LoadFromStreamExt(Stream fileStream, Stream inputStream, string fileName)
        {
            using (StreamReader reader = SysUtils.OpenStreamReader(inputStream, DEFAULT_ENCODING)) {
                fTree.Clear();
                LoadFromStream(fileStream, reader);
                fTree.Header.CharacterSet = GEDCOMCharacterSet.csASCII;
            }
        }

        private void LoadFromStream(Stream fileStream, StreamReader reader)
        {
            fTree.State = GEDCOMState.osLoading;
            try
            {
                ProgressEventHandler progressHandler = fTree.OnProgress;

                Encoding sourceEncoding = DEFAULT_ENCODING;
                EncodingState encodingState = EncodingState.esUnchecked;
                long fileSize = fileStream.Length;
                int progress = 0;

                GEDCOMCustomRecord curRecord = null;
                GEDCOMTag curTag = null;

                int lineNum = 0;
                while (reader.Peek() != -1)
                {
                    lineNum++;
                    string str = reader.ReadLine();
                    str = GEDCOMUtils.TrimLeft(str);
                    if (str.Length == 0) continue;

                    if (!GEDCOMUtils.IsDigit(str[0]))
                    {
                        FixFTBLine(curRecord, curTag, lineNum, str);
                    }
                    else
                    {
                        int tagLevel;
                        string tagXRef, tagName, tagValue;

                        try
                        {
                            str = GEDCOMUtils.ExtractNumber(str, out tagLevel, false, 0);
                            str = GEDCOMUtils.ExtractDelimiter(str, 0);
                            str = GEDCOMUtils.ExtractXRef(str, out tagXRef, true, "");
                            str = GEDCOMUtils.ExtractDelimiter(str, 0);
                            str = GEDCOMUtils.ExtractString(str, out tagName, "");
                            tagName = tagName.ToUpperInvariant();
                            str = GEDCOMUtils.ExtractDelimiter(str, 1);
                            tagValue = str;
                        }
                        catch (EGEDCOMException ex)
                        {
                            throw new EGEDCOMException("Syntax error in line " + Convert.ToString(lineNum) + ".\r" + ex.Message);
                        }

                        // convert codepages
                        if (!string.IsNullOrEmpty(tagValue) && encodingState == EncodingState.esChanged)
                        {
                            tagValue = ConvertStr(sourceEncoding, tagValue);
                        }

                        if (tagLevel == 0)
                        {
                            if (curRecord == fTree.Header && encodingState == EncodingState.esUnchecked) {
                                // beginning recognition of the first is not header record
                                // to check for additional versions of the code page
                                DefineEncoding(reader, ref sourceEncoding, ref encodingState);
                            }

                            if (tagName == "INDI")
                            {
                                curRecord = fTree.AddRecord(new GEDCOMIndividualRecord(fTree, fTree, "", ""));
                            }
                            else if (tagName == "FAM")
                            {
                                curRecord = fTree.AddRecord(new GEDCOMFamilyRecord(fTree, fTree, "", ""));
                            }
                            else if (tagName == "OBJE")
                            {
                                curRecord = fTree.AddRecord(new GEDCOMMultimediaRecord(fTree, fTree, "", ""));
                            }
                            else if (tagName == "NOTE")
                            {
                                curRecord = fTree.AddRecord(new GEDCOMNoteRecord(fTree, fTree, "", ""));
                            }
                            else if (tagName == "REPO")
                            {
                                curRecord = fTree.AddRecord(new GEDCOMRepositoryRecord(fTree, fTree, "", ""));
                            }
                            else if (tagName == "SOUR")
                            {
                                curRecord = fTree.AddRecord(new GEDCOMSourceRecord(fTree, fTree, "", ""));
                            }
                            else if (tagName == "SUBN")
                            {
                                curRecord = fTree.AddRecord(new GEDCOMSubmissionRecord(fTree, fTree, "", ""));
                            }
                            else if (tagName == "SUBM")
                            {
                                curRecord = fTree.AddRecord(new GEDCOMSubmitterRecord(fTree, fTree, "", ""));
                            }
                            else if (tagName == "_GROUP")
                            {
                                curRecord = fTree.AddRecord(new GEDCOMGroupRecord(fTree, fTree, "", ""));
                            }
                            else if (tagName == "_RESEARCH")
                            {
                                curRecord = fTree.AddRecord(new GEDCOMResearchRecord(fTree, fTree, "", ""));
                            }
                            else if (tagName == "_TASK")
                            {
                                curRecord = fTree.AddRecord(new GEDCOMTaskRecord(fTree, fTree, "", ""));
                            }
                            else if (tagName == "_COMM")
                            {
                                curRecord = fTree.AddRecord(new GEDCOMCommunicationRecord(fTree, fTree, "", ""));
                            }
                            else if (tagName == "_LOC")
                            {
                                curRecord = fTree.AddRecord(new GEDCOMLocationRecord(fTree, fTree, "", ""));
                            }
                            else if (tagName == "HEAD")
                            {
                                curRecord = fTree.Header;
                            }
                            else if (tagName == "TRLR")
                            {
                                break;
                            }
                            else
                            {
                                curRecord = null;
                            }

                            if (curRecord != null && tagXRef != "")
                            {
                                curRecord.XRef = tagXRef;
                            }
                            curTag = null;
                        }
                        else
                        {
                            if (curRecord != null)
                            {
                                if (curTag == null || tagLevel == 1)
                                {
                                    curTag = curRecord.AddTag(tagName, tagValue, null);
                                }
                                else
                                {
                                    while (tagLevel <= curTag.Level)
                                    {
                                        curTag = (curTag.Parent as GEDCOMTag);
                                    }
                                    curTag = curTag.AddTag(tagName, tagValue, null);
                                }
                            }
                        }
                    }

                    if (progressHandler != null) {
                        int newProgress = (int)Math.Min(100, (fileStream.Position * 100.0f) / fileSize);

                        if (progress != newProgress) {
                            progress = newProgress;
                            progressHandler(fTree, progress);
                        }
                    }
                }
            }
            finally
            {
                fTree.State = GEDCOMState.osReady;
            }
        }

        #endregion

        #region Saving functions

        public void SaveToFile(string fileName, GEDCOMCharacterSet charSet)
        {
            // Attention: processing of Header moved to BaseContext!

            using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                SaveToStreamExt(fileStream, fileName, charSet);
            }
        }

        public void SaveToStreamExt(Stream outputStream, string fileName, GEDCOMCharacterSet charSet)
        {
            // Attention: processing of Header moved to BaseContext!

            fTree.Pack();
            using (StreamWriter writer = new StreamWriter(outputStream, GEDCOMUtils.GetEncodingByCharacterSet(charSet))) {
                SaveToStream(writer);
                writer.Flush();
            }

            fTree.Header.CharacterSet = GEDCOMCharacterSet.csASCII;
        }

        private void SaveToStream(StreamWriter writer)
        {
            IList<GEDCOMRecord> records = fTree.GetRecords().GetList();

            SaveToStream(writer, records);
        }

        public void SaveToStream(StreamWriter writer, IList<GEDCOMRecord> list)
        {
            SaveHeaderToStream(writer);

            if (list != null)
            {
                int num = list.Count;
                for (int i = 0; i < num; i++)
                {
                    list[i].SaveToStream(writer);
                }
            }

            SaveFooterToStream(writer);
        }

        private void SaveHeaderToStream(StreamWriter stream)
        {
            fTree.Header.SaveToStream(stream);
        }

        private static void SaveFooterToStream(StreamWriter stream)
        {
            const string str = "0 TRLR";
            stream.Write(str + GEDCOMProvider.GEDCOM_NEWLINE);
        }

        #endregion

        #region Format variations

        /// <summary>
        /// Fix of errors that are in the dates of FamilyTreeBuilder.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FixFTB(string str)
        {
            string result = str;
            string su = result.Substring(0, 3).ToUpperInvariant();

            if (su == GEDCOMCustomDate.GEDCOMDateRangeArray[0] ||
                su == GEDCOMCustomDate.GEDCOMDateRangeArray[1] ||
                su == GEDCOMCustomDate.GEDCOMDateApproximatedArray[1] ||
                su == GEDCOMCustomDate.GEDCOMDateApproximatedArray[2] ||
                su == GEDCOMCustomDate.GEDCOMDateApproximatedArray[3])
            {
                result = result.Remove(0, 4);
            }
            return result;
        }

        /// <summary>
        /// Fix of line errors that are in the files of FamilyTreeBuilder.
        /// </summary>
        private static void FixFTBLine(GEDCOMCustomRecord curRecord, GEDCOMTag curTag, int lineNum, string str)
        {
            try
            {
                if (curTag is GEDCOMNotes) {
                    curTag.AddTag("CONT", str, null);
                } else {
                    if (curRecord != null) {
                        curRecord.AddTag("NOTE", str, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWrite("GEDCOMProvider.FixFTBLine(): Line " + lineNum.ToString() + " failed correct: " + ex.Message);
            }
        }

        #endregion
    }
}
