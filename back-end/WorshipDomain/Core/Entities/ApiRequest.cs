namespace WorshipDomain.Core.Entities
{
    public class ApiRequestBase
    {
        private int _length;
        public int Length
        {
            get { return _length <= 0 || _length > 100 ? 100 : _length; }
            set { _length = value; }
        }

        private int _page;
        public int Page
        {
            get { return _page <= 0 ? 1 : _page; }
            set { _page = value; }
        }

        public List<Order> Order { get; set; }

        /// <summary>
        /// Ordenação de acordo com as colunas da consulta SQL
        /// </summary>
        /// <param name="defaultSort">Ordenação padrão. Ex: Id ASC</param>
        /// <returns></returns>
        public string GetSorting(string defaultSort = "")
        {
            if (Order != null && Order.Count > 0)
            {
                var lstSort = new List<string>();
                foreach (var item in Order)
                {
                    lstSort.Add(string.Format("{0} {1}", item.Column, item.Dir.ToLower() == "desc" ? "desc" : "asc"));
                }

                return string.Join(",", lstSort);
            }

            return defaultSort;
        }
    }

    public class ApiRequest<T> : ApiRequestBase where T : class
    {
        public T Filters { get; set; }
    }

    public class Order
    {
        public string Column { get; set; }
        public string Dir { get; set; }
    }
}
