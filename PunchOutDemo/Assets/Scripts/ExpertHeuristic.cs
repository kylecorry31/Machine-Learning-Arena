﻿using MLAgents;
using System.Collections.Generic;
using UnityEngine;

public class ExpertHeuristic : Decision
{

    public float noise = 0f;
    private MLAction lastAction;
    private int actionIdx;

    private List<MLAction> moves;
    private bool shouldDodge;

    private void OnEnable()
    {
        var idx = 0; // 0 - 4
        MLAction[][] possibleMoves = new MLAction[][]{
            new MLAction[]{ MLAction.PUNCH_LEFT },
            new MLAction[]{ MLAction.PUNCH_RIGHT },
            new MLAction[]{ MLAction.PUNCH_LEFT, MLAction.PUNCH_RIGHT },
            new MLAction[]{ MLAction.PUNCH_RIGHT, MLAction.PUNCH_LEFT },
            new MLAction[]{ MLAction.PUNCH_LEFT, MLAction.PUNCH_RIGHT, MLAction.PUNCH_LEFT }
        };
        moves = new List<MLAction>(possibleMoves[idx]);
        shouldDodge = idx == 1 || idx == 3;
    }

    public override float[] Decide(List<float> vectorObs, List<Texture2D> visualObs, float reward, bool done, List<float> memory)
    {
        return RepeatActions(vectorObs,moves, shouldDodge);
    }

    public float[] RepeatActions(List<float> vectorObs, List<MLAction> actions, bool shouldDodge)
    {
        MLInput input = new MLInput(vectorObs.ToArray());

        // Sequence
        if (input.IsDodgeReady()) // Can punch / dodge
        {
            // Dodging
            if (shouldDodge && lastAction != input.GetOpponentAction() && input.GetOpponentAction() == MLAction.PUNCH_LEFT)
            {
                lastAction = input.GetOpponentAction();
                actionIdx = 0;
                return MLActionFactory.GetVectorAction(MLAction.DODGE_RIGHT);
            }

            if (shouldDodge && lastAction != input.GetOpponentAction() && input.GetOpponentAction() == MLAction.PUNCH_RIGHT)
            {
                lastAction = input.GetOpponentAction();
                actionIdx = 0;
                return MLActionFactory.GetVectorAction(MLAction.DODGE_LEFT);
            }

            lastAction = input.GetOpponentAction();
        }

        if (input.IsPunchReady())
        {
            int myComboState = input.GetMyComboState();
            if (actions.Count == 2)
            {
                switch (myComboState)
                {
                    case 0: return MLActionFactory.GetVectorAction(actions[0]);
                    case 1: return MLActionFactory.GetVectorAction(actions[1]);
                    case 2: return MLActionFactory.GetVectorAction(actions[1]);
                    default: return MLActionFactory.GetVectorAction(MLAction.NOTHING);
                }
            }
            if (actions.Count == 3)
            {
                switch (myComboState)
                {
                    case 0: return MLActionFactory.GetVectorAction(actions[0]);
                    case 1: return MLActionFactory.GetVectorAction(actions[1]);
                    case 2: return MLActionFactory.GetVectorAction(actions[1]);
                    case 3: return MLActionFactory.GetVectorAction(actions[2]);
                    case 4: return MLActionFactory.GetVectorAction(actions[2]);
                    default: return MLActionFactory.GetVectorAction(MLAction.NOTHING);
                }
            }
            return MLActionFactory.GetVectorAction(actions[0]);
        }

        return MLActionFactory.GetVectorAction(MLAction.NOTHING);
    }

    public float[] OnlyDo(List<float> vectorObs, MLAction action)
    {
        return MLActionFactory.GetVectorAction(action);
    }

    public float[] OnlyDoAndDodge(List<float> vectorObs, MLAction action)
    {
        MLInput input = new MLInput(vectorObs.ToArray());

        if (lastAction != input.GetOpponentAction() && input.GetOpponentAction() == MLAction.PUNCH_LEFT)
        {
            lastAction = input.GetOpponentAction();
            return MLActionFactory.GetVectorAction(MLAction.DODGE_RIGHT);
        }

        if (lastAction != input.GetOpponentAction() && input.GetOpponentAction() == MLAction.PUNCH_RIGHT)
        {
            lastAction = input.GetOpponentAction();
            return MLActionFactory.GetVectorAction(MLAction.DODGE_LEFT);
        }

        lastAction = input.GetOpponentAction();

        return MLActionFactory.GetVectorAction(action);
    }
   

    public float[] ExpertDecide(List<float> vectorObs)
    {
        System.Random r = new System.Random();

        if (r.NextDouble() < noise)
        {
            return new float[] { r.Next(0, 5) };
        }

        MLInput input = new MLInput(vectorObs.ToArray());

        if (input.GetOpponentAction() == MLAction.DODGE_LEFT)
        {
            return MLActionFactory.GetVectorAction(MLAction.PUNCH_RIGHT);
        }

        if (input.GetOpponentAction() == MLAction.DODGE_RIGHT)
        {
            return MLActionFactory.GetVectorAction(MLAction.PUNCH_LEFT);
        }

        if (input.GetOpponentAction() == MLAction.PUNCH_LEFT)
        {
            return MLActionFactory.GetVectorAction(MLAction.DODGE_LEFT);
        }

        if (input.GetOpponentAction() == MLAction.PUNCH_RIGHT)
        {
            return MLActionFactory.GetVectorAction(MLAction.DODGE_RIGHT);
        }

        return new float[] { 0f };
    }
    
    public override List<float> MakeMemory(List<float> vectorObs, List<Texture2D> visualObs, float reward, bool done, List<float> memory)
    {
        // DO nothing
        return memory;
    }
}
