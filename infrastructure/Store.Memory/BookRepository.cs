using System.Linq;

namespace Store.Memory
{
    public class BookRepository : IBookRepository
    {
        private readonly Book[] books = new Book[]
        {
            new Book(1, "ISBN 12312-31231", "D. Knuth", "Art Of Programming"),
            new Book(2, "ISBN 12312-31232", "M. Fowler", "Refactoring"),
            new Book(3, "ISBN 12312-31232", "B. Kernighan, D. Ritchie", "C Programming Language")
        };

        public Book[] GetAllByIsdn(string isbn)
        {
            return books.Where(book => book.Isbn == isbn)
                .ToArray();
        }

        public Book[] GetAllByTitle(string titlePart)
        {
            return books.Where(book => book.Title.Contains(titlePart))
                .ToArray();
        }

        public Book[] GetAllByTitleOrAuthor(string query)
        {
            return books.Where(book => book.Authhor.Contains(query)
                                    || book.Title.Contains(query))
                .ToArray();
        }
    }
}
