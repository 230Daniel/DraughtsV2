import React from "react";

import styles from "./Tile.Module.css";

export default class UnplayableTile extends React.Component{
	render(){
		return(
			<div className={`${styles.tile} ${styles.unplayableTile}`}/>
		)
	}
}
