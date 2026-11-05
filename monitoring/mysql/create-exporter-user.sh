#!/bin/bash
# Creates (or updates) the least-privilege MySQL user used by mysqld_exporter.
#
# Runs automatically from /docker-entrypoint-initdb.d when the mysql_data volume is empty.
# For an already-initialized volume, run it once by hand:
#   docker exec train_mysql bash /docker-entrypoint-initdb.d/create-exporter-user.sh
#
# Requires MYSQL_ROOT_PASSWORD and MYSQL_EXPORTER_PASSWORD in the container environment.
#
# The body runs in a subshell because the MySQL entrypoint *sources* non-executable
# init scripts; this keeps shell options and variables from leaking into it.
(
  set -euo pipefail

  if [ -z "${MYSQL_EXPORTER_PASSWORD:-}" ]; then
    echo "MYSQL_EXPORTER_PASSWORD is not set; cannot create the exporter user." >&2
    exit 1
  fi

  # Escape single quotes for use inside a SQL string literal.
  escaped_password=${MYSQL_EXPORTER_PASSWORD//\'/\'\'}

  mysql --protocol=socket -uroot -p"${MYSQL_ROOT_PASSWORD}" <<SQL
CREATE USER IF NOT EXISTS 'exporter'@'%' IDENTIFIED BY '${escaped_password}' WITH MAX_USER_CONNECTIONS 3;
ALTER USER 'exporter'@'%' IDENTIFIED BY '${escaped_password}' WITH MAX_USER_CONNECTIONS 3;
GRANT PROCESS, REPLICATION CLIENT, SELECT ON *.* TO 'exporter'@'%';
FLUSH PRIVILEGES;
SQL

  echo "MySQL user 'exporter' is ready."
)
