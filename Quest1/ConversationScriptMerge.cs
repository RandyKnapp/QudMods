using System;
using System.Collections.Generic;
using XRL.Core;

namespace XRL.World.Parts
{
    [Serializable]
    public class ConversationScriptMerge : IPart
    {
        public const string HAS_MERGE_CONVO_PROP = "HasMergedConversation";

        public enum MergeFlag
        {
            None,
            Append,
            Override,
            Add,
            Remove,
            UseId
        }

        public string With;
        public string IfConversationID;
        public bool PersistOriginalConversation;

        private ConversationScript _originalConvo;

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "EnteredCell");
            base.Register(Object);
        }

        public override bool FireEvent(Event E)
        {
            if (PersistOriginalConversation && E.ID == "EnteredCell" && _originalConvo != null)
            {
                if (ParentObject.GetPart<ConversationScript>() is ConversationScript currentConvo)
                {
                    if (currentConvo.ConversationID != _originalConvo.ConversationID)
                    {
                        ReattachOriginalConversation();
                    }
                }
                else
                {
                    ReattachOriginalConversation();
                }
            }
            return base.FireEvent(E);
        }

        private void ReattachOriginalConversation()
        {
            XRLCore.Log($"Reattaching Original: ParentObject={ParentObject}, OriginalConvo={_originalConvo.ConversationID}, MergeWith={With}");
            ParentObject.RemovePart<ConversationScript>();
            ParentObject.AddPart(_originalConvo);
        }

        public override void Initialize()
        {
            _originalConvo = ParentObject.GetPart<ConversationScript>();
            if (_originalConvo == null)
            {
                XRLCore.Log($"[{nameof(ConversationScriptMerge)}] ERROR: ParentObject ({ParentObject}) does not have ConversationScript part.");
                return;
            }

            if (string.IsNullOrEmpty(With))
            {
                XRLCore.Log($"[{nameof(ConversationScriptMerge)}] ERROR: 'With' parameter is empty. ParentObject ({ParentObject})");
                return;
            }

            if (!ConversationLoader.Loader.ConversationsByID.TryGetValue(With, out Conversation mergeConvo))
            {
                XRLCore.Log($"[{nameof(ConversationScriptMerge)}] ERROR: Could not find merge conversation with ID={With} ParentObject ({ParentObject})");
                return;
            }

            if (!string.IsNullOrEmpty(IfConversationID) && _originalConvo.ConversationID != IfConversationID)
            {
                return;
            }

            var currentMerges = ParentObject.GetTagOrStringProperty(HAS_MERGE_CONVO_PROP, string.Empty);
            bool hasMergedConversation = currentMerges.Contains(With);
            if (hasMergedConversation)
            {
                return;
            }

            if (!ConversationLoader.Loader.ConversationsByID.TryGetValue(_originalConvo.GetActiveConversationID(), out Conversation convoToModify))
            {
                XRLCore.Log($"[{nameof(ConversationScriptMerge)}] ERROR: Could not find conversation to modify with ID={_originalConvo.GetActiveConversationID()} ParentObject ({ParentObject})");
                return;
            }

            XRLCore.Log($"Doing Merge: ParentObject={ParentObject}, OriginalConvo={_originalConvo.ConversationID}, MergeWith={With}");
            DoMerge(_originalConvo, currentMerges, convoToModify, mergeConvo);
        }

        private void DoMerge(ConversationScript scriptToMerge, string currentMerges, Conversation convoToModify, Conversation mergeConvo)
        {
            scriptToMerge.customConversation = convoToModify;

            var allNodes = GetAllNodes(mergeConvo);
            allNodes.RemoveAll(delegate(ConversationNode node)
            {
                var mergeFlag = StripIDAndGetMergeFlag(node);
                if (TryFindNode(convoToModify, node, out ConversationNode nodeToModify))
                {
                    if (mergeFlag == MergeFlag.Override)
                    {
                        ReplaceNode(convoToModify, node);
                    }
                    else
                    {
                        MergeNode(nodeToModify, node);
                    }
                    return true;
                }

                return false;
            });

            foreach (ConversationNode node in allNodes)
            {
                AddNode(convoToModify, node);
            }

            SetThisObjectAsMerged(currentMerges, mergeConvo);
        }

        private MergeFlag StripIDAndGetMergeFlag(ConversationNode node)
        {
            var strippedId = GetTextWithMergeFlag(node.ID, out MergeFlag flags);
            node.ID = strippedId;
            return flags;
        }

        private bool TryFindNode(Conversation inConvo, ConversationNode findNode, out ConversationNode foundNode)
        {
            if (!inConvo.NodesByID.TryGetValue(findNode.ID, out foundNode))
            {
                foundNode = inConvo.StartNodes.Find(node => IsSame(node, findNode));
            }

            return foundNode != null;
        }

        private bool IsSame(ConversationNode a, ConversationNode b)
        {
            return a.ID == b.ID
                && a.IfHaveQuest == b.IfHaveQuest
                && a.IfHaveItemWithID == b.IfHaveItemWithID
                && a.IfHaveState == b.IfHaveState
                && a.IfNotHaveState == b.IfNotHaveState
                && a.IfNotHaveQuest == b.IfNotHaveQuest
                && a.IfFinishedQuest == b.IfFinishedQuest
                && a.IfFinishedQuestStep == b.IfFinishedQuestStep
                && a.IfNotFinishedQuest == b.IfNotFinishedQuest
                && a.IfHaveObservation == b.IfHaveObservation
                && a.IfHaveObservationWithTag == b.IfHaveObservationWithTag
                && a.IfHaveSultanNoteWithTag == b.IfHaveSultanNoteWithTag
                && a.IfNotFinishedQuestStep == b.IfNotFinishedQuestStep
                && a.IfWearingBlueprint == b.IfWearingBlueprint
                && a.IfHasBlueprint == b.IfHasBlueprint
                && a.IfLevelLessOrEqual == b.IfLevelLessOrEqual;
        }

        private List<ConversationNode> GetAllNodes(Conversation convo)
        {
            List<ConversationNode> result = new List<ConversationNode>(convo.StartNodes.Count + convo.NodesByID.Count);
            result.AddRange(convo.StartNodes);
            result.AddRange(convo.NodesByID.Values);
            return result;
        }

        private void ReplaceNode(Conversation convoToModify, ConversationNode node)
        {
            convoToModify.StartNodes.RemoveAll(startNode => IsSame(startNode, node));
            convoToModify.NodesByID.Remove(node.ID);
            convoToModify.AddNode(node);
        }

        private void MergeNode(ConversationNode nodeToModify, ConversationNode node)
        {
            MergeNodeAttributes(nodeToModify, node);
            MergeNodeText(nodeToModify, node);
            MergeNodeChoices(nodeToModify, node);
        }

        private void MergeNodeAttributes(ConversationNode nodeToModify, ConversationNode node)
        {
            nodeToModify.IfHaveQuest = OverrideAttribute(node.IfHaveQuest, nodeToModify.IfHaveQuest);
            nodeToModify.IfHaveItemWithID = OverrideAttribute(node.IfHaveItemWithID, nodeToModify.IfHaveItemWithID);
            nodeToModify.IfHaveState = OverrideAttribute(node.IfHaveState, nodeToModify.IfHaveState);
            nodeToModify.IfNotHaveState = OverrideAttribute(node.IfNotHaveState, nodeToModify.IfNotHaveState);
            nodeToModify.IfNotHaveQuest = OverrideAttribute(node.IfNotHaveQuest, nodeToModify.IfNotHaveQuest);
            nodeToModify.IfFinishedQuest = OverrideAttribute(node.IfFinishedQuest, nodeToModify.IfFinishedQuest);
            nodeToModify.IfFinishedQuestStep = OverrideAttribute(node.IfFinishedQuestStep, nodeToModify.IfFinishedQuestStep);
            nodeToModify.IfNotFinishedQuest = OverrideAttribute(node.IfNotFinishedQuest, nodeToModify.IfNotFinishedQuest);
            nodeToModify.IfHaveObservation = OverrideAttribute(node.IfHaveObservation, nodeToModify.IfHaveObservation);
            nodeToModify.IfHaveObservationWithTag = OverrideAttribute(node.IfHaveObservationWithTag, nodeToModify.IfHaveObservationWithTag);
            nodeToModify.IfHaveSultanNoteWithTag = OverrideAttribute(node.IfHaveSultanNoteWithTag, nodeToModify.IfHaveSultanNoteWithTag);
            nodeToModify.IfNotFinishedQuestStep = OverrideAttribute(node.IfNotFinishedQuestStep, nodeToModify.IfNotFinishedQuestStep);
            nodeToModify.StartQuest = OverrideAttribute(node.StartQuest, nodeToModify.StartQuest);
            nodeToModify.FinishQuest = OverrideAttribute(node.FinishQuest, nodeToModify.FinishQuest);
            nodeToModify.RevealMapNoteId = OverrideAttribute(node.RevealMapNoteId, nodeToModify.RevealMapNoteId);
            nodeToModify.CompleteQuestStep = OverrideAttribute(node.CompleteQuestStep, nodeToModify.CompleteQuestStep);
            nodeToModify.GiveItem = OverrideAttribute(node.GiveItem, nodeToModify.GiveItem);
            nodeToModify.GiveOneItem = OverrideAttribute(node.GiveOneItem, nodeToModify.GiveOneItem);
            nodeToModify.TakeItem = OverrideAttribute(node.TakeItem, nodeToModify.TakeItem);
            nodeToModify.ClearOwner = OverrideAttribute(node.ClearOwner, nodeToModify.ClearOwner);
            nodeToModify.IfWearingBlueprint = OverrideAttribute(node.IfWearingBlueprint, nodeToModify.IfWearingBlueprint);
            nodeToModify.IfHasBlueprint = OverrideAttribute(node.IfHasBlueprint, nodeToModify.IfHasBlueprint);
            nodeToModify.IfLevelLessOrEqual = OverrideAttribute(node.IfLevelLessOrEqual, nodeToModify.IfLevelLessOrEqual);
            nodeToModify.SpecialRequirement = OverrideAttribute(node.SpecialRequirement, nodeToModify.SpecialRequirement);
            nodeToModify.Filter = OverrideAttribute(node.Filter, nodeToModify.Filter);
        }

        private string OverrideAttribute(string a, string b)
        {
            return string.IsNullOrEmpty(a) ? b : a;
        }

        private void MergeNodeText(ConversationNode nodeToModify, ConversationNode node)
        {
            if (!string.IsNullOrEmpty(node.Text))
            {
                var text = GetTextWithMergeFlag(node.Text, out MergeFlag mergeFlag);
                if (mergeFlag == MergeFlag.Append)
                {
                    nodeToModify.Text += text;
                }
                else
                {
                    nodeToModify.Text = text;
                }
            }
        }

        private void MergeNodeChoices(ConversationNode nodeToModify, ConversationNode node)
        {
            var choices = new List<ConversationChoice>(node.Choices);
            choices.RemoveAll(delegate (ConversationChoice choice) {
                choice.ID = GetTextWithMergeFlag(choice.ID, out MergeFlag mergeFlag);
                if (mergeFlag == MergeFlag.None)
                {
                    choice.GotoID = GetTextWithMergeFlag(choice.GotoID, out mergeFlag);
                }
                if (TryFindChoice(nodeToModify, choice, out ConversationChoice foundChoice))
                {
                    switch (mergeFlag)
                    {
                        case MergeFlag.Add:
                            return false;
                        case MergeFlag.Remove:
                            RemoveChoice(nodeToModify, choice);
                            break;
                        case MergeFlag.UseId:
                            DoUseId(nodeToModify, choice);
                            break;
                        case MergeFlag.Append:
                            MergeChoiceText(foundChoice, choice);
                            break;
                        default:
                            ReplaceChoice(nodeToModify, foundChoice, choice);
                            break;
                    }
                    return true;
                }

                return false;
            });

            foreach (var choice in choices)
            {
                AddChoice(nodeToModify, choice);
            }

            nodeToModify.SortEndChoicesToEnd();
            ResetOrdinals(nodeToModify);
        }

        private void ResetOrdinals(ConversationNode nodeToModify)
        {
            for (int index = 0; index < nodeToModify.Choices.Count; index++)
            {
                var choice = nodeToModify.Choices[index];
                choice.Ordinal = index;
            }
        }

        private void DoUseId(ConversationNode node)
        {
            for (int index = 0; index < node.Choices.Count; index++)
            {
                var choice = node.Choices[index];
                GetTextWithMergeFlag(choice.ID, out MergeFlag mergeFlag);
                if (mergeFlag == MergeFlag.UseId)
                {
                    DoUseId(node, choice);
                }
            }
        }

        private void DoUseId(ConversationNode nodeToModify, ConversationChoice choice)
        {
            var useId = choice.ID;
            choice.ID = string.Empty;

            var foundChoice = GetChoiceById(nodeToModify.ParentConversation, useId);
            if (foundChoice != null)
            {
                var copy = new ConversationChoice();
                copy.Copy(foundChoice);
                ReplaceChoice(nodeToModify, choice, copy);
            }
        }

        private void ReplaceChoice(ConversationNode nodeToModify, ConversationChoice choice, ConversationChoice with)
        {
            var index = nodeToModify.Choices.FindIndex(c => IsSame(c, choice));
            if (index >= 0)
            {
                nodeToModify.Choices.RemoveAt(index);
                nodeToModify.Choices.Insert(index, with);
            }
        }

        private ConversationChoice GetChoiceById(Conversation convo, string useId)
        {
            var allNodes = GetAllNodes(convo);
            foreach (var node in allNodes)
            {
                var foundChoice = node.Choices.Find(c => c.ID == useId);
                if (foundChoice != null)
                {
                    return foundChoice;
                }
            }

            return null;
        }

        private bool TryFindChoice(ConversationNode nodeToModify, ConversationChoice choice, out ConversationChoice foundChoice)
        {
            foundChoice = nodeToModify.Choices.Find(c => IsSame(c, choice));
            return foundChoice != null;
        }

        private bool IsSame(ConversationChoice a, ConversationChoice b)
        {
            return a.ID == b.ID && a.GotoID == b.GotoID;
        }

        private void AddChoice(ConversationNode nodeToModify, ConversationChoice choice)
        {
            nodeToModify.Choices.Add(choice);
        }

        /*private void MergeChoice(ConversationChoice choiceToModify, ConversationChoice choice)
        {
            choiceToModify.Achievement = OverrideAttribute(choice.Achievement, choiceToModify.Achievement);
            choiceToModify.IfHaveState = OverrideAttribute(choice.IfHaveState, choiceToModify.IfHaveState);
            choiceToModify.IfNotHaveState = OverrideAttribute(choice.IfNotHaveState, choiceToModify.IfNotHaveState);
            choiceToModify.IfHavePart =OverrideAttribute(choice.IfHavePart, choiceToModify.IfHavePart);
            choiceToModify.IfNotHavePart = OverrideAttribute(choice.IfNotHavePart, choiceToModify.IfNotHavePart);
            choiceToModify.IfHaveQuest = OverrideAttribute(choice.IfHaveQuest, choiceToModify.IfHaveQuest);
            choiceToModify.IfHaveItemWithID = OverrideAttribute(choice.IfHaveItemWithID, choiceToModify.IfHaveItemWithID);
            choiceToModify.IfNotHaveQuest = OverrideAttribute(choice.IfNotHaveQuest, choiceToModify.IfNotHaveQuest);
            choiceToModify.IfFinishedQuest = OverrideAttribute(choice.IfFinishedQuest, choiceToModify.IfFinishedQuest);
            choiceToModify.IfFinishedQuestStep = OverrideAttribute(choice.IfFinishedQuestStep, choiceToModify.IfFinishedQuestStep);
            choiceToModify.IfNotFinishedQuest = OverrideAttribute(choice.IfNotFinishedQuest, choiceToModify.IfNotFinishedQuest);
            choiceToModify.IfNotFinishedQuestStep = OverrideAttribute(choice.IfNotFinishedQuestStep, choiceToModify.IfNotFinishedQuestStep);
            choiceToModify.IfHasBlueprint = OverrideAttribute(choice.IfHasBlueprint, choiceToModify.IfHasBlueprint);
            choiceToModify.SpecialRequirement = OverrideAttribute(choice.SpecialRequirement, choiceToModify.SpecialRequirement);
            choiceToModify.IfWearingBlueprint = OverrideAttribute(choice.IfWearingBlueprint, choiceToModify.IfWearingBlueprint);

            MergeChoiceText(choiceToModify, choice);
        }*/

        private void MergeChoiceText(ConversationChoice choiceToModify, ConversationChoice choice)
        {
            if (!string.IsNullOrEmpty(choice.Text))
            {
                var text = GetTextWithMergeFlag(choice.Text, out MergeFlag mergeFlag);
                if (mergeFlag == MergeFlag.Append)
                {
                    choiceToModify.Text += text;
                }
                else
                {
                    choiceToModify.Text = text;
                }
            }
        }

        private void RemoveChoice(ConversationNode nodeToModify, ConversationChoice choice)
        {
            nodeToModify.Choices.RemoveAll(c => IsSame(c, choice));
        }

        private string GetTextWithMergeFlag(string text, out MergeFlag outFlag)
        {
            outFlag = MergeFlag.None;

            var enumNames = Enum.GetNames(typeof(MergeFlag));
            var enumValues = Enum.GetValues(typeof(MergeFlag));
            for (int index = 0; index < enumNames.Length; index++)
            {
                var flagName = $"%{enumNames[index].ToUpperInvariant()}%";
                var flag = (MergeFlag)enumValues.GetValue(index);

                if (text.Contains(flagName))
                {
                    XRLCore.Log("Found Flag: " + flagName);
                    outFlag = flag;
                    text = text.Replace(flagName, string.Empty);
                }
            }

            return text;
        }

        private void AddNode(Conversation convoToModify, ConversationNode node)
        {
            convoToModify.AddNode(node);
            DoUseId(node);
        }

        private void SetThisObjectAsMerged(string currentMerges, Conversation mergeConvo)
        {
            if (string.IsNullOrEmpty(currentMerges))
            {
                currentMerges = mergeConvo.ID;
            }
            else
            {
                currentMerges += ";" + mergeConvo.ID;
            }

            ParentObject.SetStringProperty(HAS_MERGE_CONVO_PROP, currentMerges + ";" + mergeConvo.ID);
        }
    }
}