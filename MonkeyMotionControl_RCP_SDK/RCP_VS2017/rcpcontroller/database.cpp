/********************************************************************************
 * This file is part of the RCP SDK Release 6.62.0
 * Copyright (C) 2009-2018 RED.COM, LLC.  All rights reserved.
 *
 * For technical support please email rcpsdk@red.com.
 *
 * "Source Code" means the accompanying software in any form preferred for making
 * modifications. "Source Code" does not include the accompanying strlcat.c and
 * strlcpy.c software and examples/qt/common/qt/serial software.
 * 
 * "Binary Code" means machine-readable Source Code in binary form.
 * 
 * "Approved Recipients" means only those recipients of the Source Code who have
 * entered into the RCP SDK License Agreement with RED.COM, LLC. All
 * other recipients are not authorized to possess, modify, use, or distribute the
 * Source Code.
 *
 * RED.COM, LLC hereby grants Approved Recipients the rights to modify this
 * Source Code, create derivative works based on this Source Code, and distribute
 * the modified/derivative works only as Binary Code in its binary form. Approved
 * Recipients may not distribute the Source Code or any modification or derivative
 * work of the Source Code. Redistributions of Binary Code must reproduce this
 * copyright notice, this list of conditions, and the following disclaimer in the
 * documentation or other materials provided with the distribution. RED.COM, LLC
 * may not be used to endorse or promote Binary Code redistributions without
 * specific prior written consent from RED.COM, LLC. 
 *
 * The only exception to the above licensing requirements is any recipient may use,
 * copy, modify, and distribute in any format the strlcat.c and strlcpy.c software
 * in accordance with the provisions required by the license associated therewith;
 * provided, however, that the modifications are solely to the strlcat.c and
 * strlcpy.c software and do not include any other portion of the Source Code.
 * 
 * THE ACCOMPANYING SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, TITLE, AND NON-INFRINGEMENT.
 * IN NO EVENT SHALL THE RED.COM, LLC, ANY OTHER COPYRIGHT HOLDERS, OR ANYONE
 * DISTRIBUTING THE SOFTWARE BE LIABLE FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER
 * IN CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 ********************************************************************************/

#include <stdexcept>
#include "database.h"
#include "logger.h"
#include "see_keys/see_keys.h"

Database::Database(const QString &filename, bool encrypt /*= true*/, int flags /*= SQLITE_OPEN_READWRITE|SQLITE_OPEN_CREATE*/)
{
    m_name = filename;
    m_rowCount = 0;
    m_colCount = 0;

    //bool isFactory = m_name.contains("factory") && m_name.endsWith(".db");
    bool isFactory = m_name.endsWith(".db");
    bool isPreset = m_name.endsWith(".preset");
    bool isNewDb = (flags & SQLITE_OPEN_CREATE) == SQLITE_OPEN_CREATE;

    size_t i;
    size_t keyCount = sizeof(see_keys) / sizeof(see_keys[0]);

    for (i = 0; i < keyCount; i++)
    {
        if (sqlite3_open_v2(m_name.toUtf8().constData(), &m_db, flags, 0) != SQLITE_OK)
        {
            close();
            Logger::logError(QString("Unable to open database %1 correctly. SQLite error: %2").arg(m_name).arg(sqlite3_errmsg(m_db)), "Database::Database(const QString&, bool, int)");
            return;
        }

        if (encrypt)
        {
            if (sqlite3_key(m_db, see_keys[i], strlen(see_keys[i])) != SQLITE_OK)
            {
                close();
                Logger::logError(QString("Unable to perform sqlite3_key() on %1 correctly. SQLite error: %2").arg(m_name).arg(sqlite3_errmsg(m_db)), "Database::Database(const QString&, bool, int)");
                return;
            }
        }

        if (isNewDb)
        {
            m_isOpen = true;
            Logger::logInfo(QString("Database %1 opened.").arg(m_name), "Database::Database(const QString&, bool, int)");
            return;
        }
        else if (isFactory)
        {
            if (sqlite3_exec(m_db, "SELECT * FROM key_actions", 0, 0, 0) == SQLITE_OK)
            {
                m_isOpen = true;
                return;
            }
            else if (sqlite3_exec(m_db, "SELECT * FROM parameters", 0, 0, 0) == SQLITE_OK)
            {
                m_isOpen = true;
                return;
            }
        }
        else if (isPreset)
        {
            if (sqlite3_exec(m_db, "SELECT * FROM info", 0, 0, 0) == SQLITE_OK)
            {
                m_isOpen = true;
                return;
            }
        }

        close();
    }

    Logger::logError(QString("Database %1 was not opened. No encryption keys were valid.").arg(m_name), "Database::Database(const QString&, bool, int)");
}

Database::~Database()
{
    close();
}

int Database::rowCount() const
{
    return m_rowCount;
}

int Database::colCount() const
{
    return m_colCount;
}

bool Database::isOpen() const
{
    return m_isOpen;
}

// Retrieves an item (0-indexed) from the result set of a SELECT statement at a given row and column.
// If the item does not exist, an empty string is returned.
QString Database::itemAt(int row, int col) const
{
    if (m_results.empty())
    {
        Logger::logWarning(QString("Item at (R:%1, C:%2) does not exist. The result set is empty.").arg(row).arg(col), "Database::itemAt(int, int)");
    }
    else
    {
        if (row >= m_rowCount || col >= m_colCount)
        {
            Logger::logWarning(QString("Item at (R:%1, C:%2) does not exist. Row count: %3 | Column count: %4").arg(row).arg(col).arg(m_rowCount).arg(m_colCount), "Database::itemAt(int, int)");
        }
        else
        {
            try
            {
                std::list<QString> properties = m_results.at(row);
                std::list<QString>::iterator i = properties.begin();
                int count = 0;

                while (count < col)
                {
                    count++;
                    i++;
                }

                return *i;
            }
            catch (std::out_of_range &oor)
            {
                Logger::logWarning(QString("An out_of_range error occurred. R%1 does not exist. Exception: %2").arg(row).arg(oor.what()), "Database::itemAt(int, int)");
            }
        }
    }

    return "";
}

// Copies the current result set into results.
// Useful if you want to use multiple result sets at once, since an instance of this class only stores the most recent result set from a SELECT query.
void Database::copyResults(std::map<int, std::list<QString> > &results) const
{
    int i, j;

    for (i = 0; i < m_rowCount; i++)
    {
        for (j = 0; j < m_colCount; j++)
        {
            results[i].push_back(itemAt(i, j));
        }
    }
}

bool Database::executeQuery(const QString &query)
{
    sqlite3_stmt *statement;

    if (sqlite3_prepare_v2(m_db, query.toUtf8().constData(), -1, &statement, 0) != SQLITE_OK)
    {
        Logger::logError(QString("Unable to prepare query \"%1\" correctly. SQLite error: %2").arg(query).arg(sqlite3_errmsg(m_db)), "Database::executeQuery(const QString&)");
        return false;
    }


    Logger::logInfo(QString("Executing query \"%1\" against %2").arg(query).arg(m_name), "Database::executeQuery(const QString&)");

    if (query.startsWith("SELECT", Qt::CaseInsensitive))
    {
        clearResults();

        int i;
        m_colCount = sqlite3_column_count(statement);

        while (sqlite3_step(statement) == SQLITE_ROW)
        {
            for (i = 0; i < m_colCount; i++)
            {
                m_results[m_rowCount].push_back(QString((const char*)sqlite3_column_text(statement, i)));
            }

            m_rowCount++;
        }
    }
    else
    {
        if (sqlite3_step(statement) != SQLITE_DONE)
        {
            Logger::logError(QString("An issue occurred while executing the query \"%1\". SQLite error: %2").arg(query).arg(sqlite3_errmsg(m_db)), "Database::executeQuery(const QString&)");
            return false;
        }
    }

    sqlite3_finalize(statement);
    return true;
}

bool Database::executeQueries(const QString &queries)
{
    if (sqlite3_exec(m_db, queries.toUtf8().constData(), 0, 0, 0) != SQLITE_OK)
    {
        Logger::logError(QString("An error occurred while executing the queries \"%1\". SQLite error: %2").arg(queries).arg(sqlite3_errmsg(m_db)), "Database::executeQueries(const QString&)");
        return false;
    }

    return true;
}

void Database::clearResults()
{
    int i;
    for (i = 0; i < m_rowCount; i++)
    {
        try
        {
            m_results.at(i).clear();
        }
        catch (std::out_of_range &oor)
        {
            Logger::logWarning(QString("An out of range error occurred while clearing the result set. Exception: %1").arg(oor.what()), "Database::clearResults()");
        }
    }

    m_results.clear();

    m_rowCount = 0;
    m_colCount = 0;
}

void Database::close()
{
    if (m_db != 0)
    {
        sqlite3_close(m_db);
        m_db = 0;
        Logger::logInfo(QString("Database %1 closed.").arg(m_name), "Database::close()");
    }

    m_isOpen = false;
}
