using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviorTree : MonoBehaviour
{
    private BTNode root;
    private Blackboard blackboard;
    
    protected virtual void Start()
    {
        blackboard = new Blackboard();
        root = SetupTree();
    }

    protected virtual void Update()
    {
        if (root != null)
            root.Evaluate();
    }

    protected abstract BTNode SetupTree();

    public Blackboard GetBlackboard() => blackboard;
}

public class Blackboard
{
    private Dictionary<string, object> data = new Dictionary<string, object>();

    public void SetValue(string key, object value) => data[key] = value;
    
    public T GetValue<T>(string key, T defaultValue = default)
    {
        if (data.ContainsKey(key) && data[key] is T)
            return (T)data[key];
        return defaultValue;
    }
}

public abstract class BTNode
{
    public enum NodeState { Running, Success, Failure }
    protected NodeState state;
    
    public abstract NodeState Evaluate();
}

public class Sequence : BTNode
{
    private List<BTNode> children = new List<BTNode>();
    
    public Sequence(List<BTNode> children) => this.children = children;
    
    public override NodeState Evaluate()
    {
        bool anyChildRunning = false;
        
        foreach (var child in children)
        {
            switch (child.Evaluate())
            {
                case NodeState.Running:
                    anyChildRunning = true;
                    break;
                case NodeState.Failure:
                    state = NodeState.Failure;
                    return state;
                case NodeState.Success:
                    continue;
            }
        }
        
        state = anyChildRunning ? NodeState.Running : NodeState.Success;
        return state;
    }
}

public class Selector : BTNode
{
    private List<BTNode> children = new List<BTNode>();
    
    public Selector(List<BTNode> children) => this.children = children;
    
    public override NodeState Evaluate()
    {
        foreach (var child in children)
        {
            switch (child.Evaluate())
            {
                case NodeState.Running:
                    state = NodeState.Running;
                    return state;
                case NodeState.Success:
                    state = NodeState.Success;
                    return state;
                case NodeState.Failure:
                    continue;
            }
        }
        
        state = NodeState.Failure;
        return state;
    }
}

public class Condition : BTNode
{
    public delegate bool ConditionDelegate();
    private ConditionDelegate condition;
    
    public Condition(ConditionDelegate condition) => this.condition = condition;
    
    public override NodeState Evaluate()
    {
        state = condition() ? NodeState.Success : NodeState.Failure;
        return state;
    }
}

public class ActionNode : BTNode
{
    public delegate NodeState ActionDelegate();
    private ActionDelegate action;
    
    public ActionNode(ActionDelegate action) => this.action = action;
    
    public override NodeState Evaluate()
    {
        state = action();
        return state;
    }
}

public class Parallel : BTNode
{
    private List<BTNode> children = new List<BTNode>();
    
    public Parallel(List<BTNode> children) => this.children = children;
    
    public override NodeState Evaluate()
    {
        int successCount = 0;
        int failureCount = 0;
        
        foreach (var child in children)
        {
            switch (child.Evaluate())
            {
                case NodeState.Success:
                    successCount++;
                    break;
                case NodeState.Failure:
                    failureCount++;
                    break;
            }
        }
        
        if (successCount == children.Count)
            state = NodeState.Success;
        else if (failureCount > 0)
            state = NodeState.Failure;
        else
            state = NodeState.Running;
            
        return state;
    }
}