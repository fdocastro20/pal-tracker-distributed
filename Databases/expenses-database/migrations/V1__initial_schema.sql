create table expenses (
  id   bigint(20) not null auto_increment,
  project_id bigint(20),
  user_id bigint(20),
  date datetime,
  expense_type VARCHAR(255),
  amount DECIMAL(13,2),

  primary key (id)
)
engine = innodb
default charset = utf8;
