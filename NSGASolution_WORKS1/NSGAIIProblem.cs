﻿using JMetalCSharp.Core;
using JMetalCSharp.Encoding.SolutionType;
using JMetalCSharp.Utils;
using JMetalCSharp.Utils.Wrapper;
using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System.Windows.Forms;


namespace NSGASolution_WORKS1
{
    /// <summary>
    /// 
    /// </summary>
    public class NSGAIIProblem : Problem
    {
        NSGASolutionComponent component = null;
        List<double> var1Value = new List<double>();
        List<double> var2Value = new List<double>();
        List<double> objectiveValue = new List<double>();
        List<GH_NumberSlider> variablesSliders = new List<GH_NumberSlider>();
        List<double> objectives = new List<double>();
        int solutionsCounter;

        #region Constructors

        /// <summary>
        /// Constructor.
        /// Creates a new multiobjective problem instance.
        /// </summary>
        /// <param name="solutionType">The solution type must "Real" or "BinaryReal", and "ArrayReal".</param>
        /// <param name="numberOfVariables">Number of variables</param>
        public NSGAIIProblem(string solutionType, NSGASolutionComponent comp, int solutionsCounter)
        {
            this.solutionsCounter = solutionsCounter;
            this.component = comp;
            NumberOfVariables = comp.readSlidersList().Count;
            NumberOfObjectives = comp.objectives.Count;
            NumberOfConstraints = 0;
            ProblemName = "Multiobjective";


            UpperLimit = new double[NumberOfVariables];
            LowerLimit = new double[NumberOfVariables];

            for (int i = 0; i < NumberOfVariables; i++)
            {
                GH_NumberSlider curSlider = comp.readSlidersList()[i];
                
                LowerLimit[i] = (double)curSlider.Slider.Minimum;
                UpperLimit[i] = (double)curSlider.Slider.Maximum; 
            }

            if (solutionType == "BinaryReal")
            {
                SolutionType = new BinaryRealSolutionType(this);
            }
            else if (solutionType == "Real")
            {
                SolutionType = new RealSolutionType(this);
            }
            else if (solutionType == "ArrayReal")
            {
                SolutionType = new ArrayRealSolutionType(this);
            }
            else
            {
                Console.WriteLine("Error: solution type " + solutionType + " is invalid");
                //Logger.Log.Error("Solution type " + solutionType + " is invalid");
                return;
            }

        }

        #endregion

        public override void Evaluate(Solution solution)
        {
            double[] storeVar = new double[NumberOfVariables];
            double[] storeObj = new double[NumberOfObjectives];
            XReal x = new XReal(solution);

            // Reading x values
            double[] xValues = new double[NumberOfVariables];
            for (int i = 0; i < NumberOfVariables; i++)
            {
                xValues[i] = x.GetValue(i);
                var1Value.Add(x.GetValue(0));
                var2Value.Add(x.GetValue(1));

            }

            GH_NumberSlider currentSlider = null;
            for (int i = 0; i < component.readSlidersList().Count; i++)
            {
                currentSlider = component.readSlidersList()[i];
                currentSlider.SetSliderValue((decimal)x.GetValue(i));
                //component.allSolutions = component.allSolutions + "" + x.GetValue(i) + " ";
                Grasshopper.Instances.ActiveCanvas.Document.NewSolution(true);
            }


            for (int i = 0; i < component.objectives.Count; i++)
            {
                solution.Objective[i] = component.objectives[i];
                //component.allSolutions = component.allSolutions +"" + component.objectives[i] + " ";

            }

            //component.allSolutions = component.allSolutions + "\n"; 


            //EvaluateObjectives(component);

            //Grasshopper.Instances.ActiveCanvas.Document.ScheduleSolution(3, (GH_Document doc) =>
            //{
            //  // Reading objectives
            //    for (int i = 0; i < NumberOfObjectives; i++)
            //    {
            //        List<double> newObj = component.getObjectives();
            //        solution.Objective[i] = newObj[i];
            //        objectiveValue.Add(newObj[0]);
            //    }
            //});
        }



        #region Private Region

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double EvalG(XReal x)
        {
            double g = 0.0;
            for (int i = 1; i < x.GetNumberOfDecisionVariables(); i++)
            {
                g += x.GetValue(i);
            }
            double constant = (9.0 / (NumberOfVariables - 1));
            g = constant * g;
            g = g + 1.0;
            return g;
        }

        public double EvalH(double f, double g)
        {
            double h = 0.0;
            h = 1.0 - Math.Sqrt(f / g) - (f / g) * Math.Sin(10.0 * Math.PI * f);
            return h;
        }

        #endregion

        public List<double> getObjectiveValues()
        {
            return objectives;
        }
        public List<double> getVar1Values()
        {
            return var1Value;
        }
        public List<double> getVar2Values()
        {
            return var2Value;
        }
    }
}
