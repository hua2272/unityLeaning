using UnityEngine;
using System.Data;
using System.Collections.Generic;

public class DialogueLoader : MonoBehaviour
{
    public DialogueNode LoadDialogueNode(int npcId, int nodeId = 1)
    {
        DialogueNode node = new DialogueNode();
        node.options = new List<DialogueOption>();

        string query = $@"
            SELECT n.*, o.option_id, o.option_text, o.next_node_id, 
                   o.required_item_id, o.required_quest_progress
            FROM dialogue_nodes n
            LEFT JOIN dialogue_options o ON n.node_id = o.node_id
            WHERE n.node_id = {nodeId} AND (n.npc_id = {npcId} OR n.npc_id = 0)";

        IDataReader reader = DatabaseManager.instance.ExecuteQuery(query);
        bool firstRow = true;
        while (reader.Read())
        {
            if (firstRow)
            {
                node.nodeId = reader.GetInt32(reader.GetOrdinal("node_id"));
                node.speakerName = reader.GetString(reader.GetOrdinal("speaker_name"));
                node.dialogueText = reader.GetString(reader.GetOrdinal("dialogue_text"));
                node.isPlayerDialogue = reader.GetInt32(reader.GetOrdinal("is_player_dialogue"));
                //Debug.Log("<<<<<<load: " + reader.GetString(reader.GetOrdinal("speaker_name")));
                firstRow = false;
            }
            if (!reader.IsDBNull(reader.GetOrdinal("option_id")))
            {
                DialogueOption option = new DialogueOption
                {
                    optionId = reader.GetInt32(reader.GetOrdinal("option_id")),
                    optionText = reader.GetString(reader.GetOrdinal("option_text")),
                    nextNodeId = reader.GetInt32(reader.GetOrdinal("next_node_id")),
                    requiredItemId = reader.IsDBNull(reader.GetOrdinal("required_item_id")) ? 0 : reader.GetInt32(reader.GetOrdinal("required_item_id")),
                    requiredQuestProgress = reader.IsDBNull(reader.GetOrdinal("required_quest_progress")) ? 0 : reader.GetInt32(reader.GetOrdinal("required_quest_progress")),
                };
                //Debug.Log("<<<<<<load: " + reader.GetString(reader.GetOrdinal("option_text")));
                node.options.Add(option);
            }
        }
        reader.Close();
        return node;
    }
}