CREATE TABLE parcel_partition.parcel (
	parcel_id int8 NOT NULL,
	weight int4 NOT NULL,
	part_count int2 NOT NULL,
	CONSTRAINT parcel_pk PRIMARY KEY (parcel_id)
);

CREATE TABLE parcel_partition.parcel_partition (
	id int8 NOT NULL,
	box_id int8 NOT NULL,
	part_weight int4 NOT NULL,
	part_cost int8 NOT NULL,
	CONSTRAINT parcel_partition_pk PRIMARY KEY (id)
);
