using DevExpress.Data.Filtering;
using DevExpress.Data.Filtering.Helpers;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MesAdmin.Common.Utils
{
    public class RestrictEditingHelper : Behavior<TableView> {
        RestrictEditingConditionCollection ConditionsCore = new RestrictEditingConditionCollection();
        public RestrictEditingConditionCollection Conditions { get { return ConditionsCore; } }

        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.ShowingEditor += OnShowingEditor;
        }
        protected override void OnDetaching() {
            AssociatedObject.ShowingEditor -= OnShowingEditor;
            base.OnDetaching();
        }
        void OnShowingEditor(object sender, ShowingEditorEventArgs e) {
            var conditions = Conditions.Where(x => x.FieldName == e.Column.FieldName).Select(y => y.Expression);
            if (conditions.Count() == 0)
                return;
            var properties = TypeDescriptor.GetProperties(e.Row);
            foreach (var condition in conditions) {
                ExpressionEvaluator ev = new ExpressionEvaluator(properties, CriteriaOperator.Parse(condition));
                var result = ev.Evaluate(e.Row);
                if (result is bool && (bool)result) {
                    e.Cancel = true;
                    return;
                }
            }
        }
    }
    public class RestrictEditingHelper2 : Behavior<TreeListView>
    {
        RestrictEditingConditionCollection ConditionsCore = new RestrictEditingConditionCollection();
        public RestrictEditingConditionCollection Conditions { get { return ConditionsCore; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ShowingEditor += OnShowingEditor;
        }
        protected override void OnDetaching()
        {
            AssociatedObject.ShowingEditor -= OnShowingEditor;
            base.OnDetaching();
        }
        void OnShowingEditor(object sender, DevExpress.Xpf.Grid.TreeList.TreeListShowingEditorEventArgs e)
        {
            var conditions = Conditions.Where(x => x.FieldName == e.Column.FieldName).Select(y => y.Expression);
            if (conditions.Count() == 0)
                return;
            var properties = TypeDescriptor.GetProperties(e.Node.Content);
            foreach (var condition in conditions)
            {
                ExpressionEvaluator ev = new ExpressionEvaluator(properties, CriteriaOperator.Parse(condition));
                var result = ev.Evaluate(e.Node.Content);
                if (result is bool && (bool)result)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }
    }

    public class RestrictEditingConditionCollection : List<RestrictEditingCondition> { }
    public class RestrictEditingCondition {
        public string FieldName { get; set; }
        public string Expression { get; set; }
    }
}