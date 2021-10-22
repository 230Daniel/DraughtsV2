import React from "react";

import styles from "./Piece.Module.css";

export default class Piece extends React.Component {
	render() {
		// If the piece passed is -1 (empty), we don't need to render anything
		if (this.props.piece === -1)
			return null;

		// Render the piece, and if it's a king (value is 2 or 3) add a king div for the crown.
		return (
			<div className={this.getClassNames()}>
				<div className={styles.inside}>
					{this.props.piece >= 2 &&
						<div className={styles.king} />
					}
				</div>
			</div>
		);
	}

	// Returns a className based on this component's props
	// Eg. if "selected=true" is passed then the selected class is added
	getClassNames() {
		var classNames = [styles.piece, this.props.piece % 2 === 0 ? styles.black : styles.white];
		if (this.props.selected) classNames.push(styles.selected);
		return classNames.join(" ");
	}
}
