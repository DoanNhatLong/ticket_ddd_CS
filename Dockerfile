version: '3.8'

services:
  mysql-db:
    image: mysql:8.0
    container_name: train_mysql
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: ad1412
      MYSQL_DATABASE: 0test1
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql

  redis-cache:
    image: redis:alpine
    container_name: train_redis
    restart: always
    ports:
      - "6379:6379"

volumes:
  mysql_data:
